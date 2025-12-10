using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminSystem.Web.Controllers
{
    public class ContactController(IUnitOfWork _unitOfWork, IMapper _mapper, IMemoryCache _cache) : Controller
    {
        public async Task<IActionResult> Index(SearchDto searchDto)
        {
            var query = _unitOfWork.Contacts.Get();

            // 搜尋
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                string term = searchDto.SearchTerm.ToUpper();
                query = query.Where(c =>
                    c.姓名.Contains(term) ||
                    c.Email.Contains(term) ||
                    (c.職稱 != null && c.職稱.Contains(term)) ||
                    (c.手機 != null && c.手機.Contains(term)) ||
                    (c.電話 != null && c.電話.Contains(term)));
            }

            // 動態排序
            string orderBy = searchDto.OrderName;
            if (searchDto.Order == Enums.Order.desc)
                orderBy += " descending";

            query = query.OrderBy(orderBy);

            // 總筆數
            var totalRecords = await query.CountAsync();

            // 分頁
            var items = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            var vm = new PagedResultDto<客戶聯絡人>
            {
                Items = items,
                TotalRecords = totalRecords,
                SearchDto = searchDto
            };

            // 為 View 準備下拉選單（職稱）
            ViewBag.JobTitles = new SelectList(
                await _unitOfWork.Contacts.Get().Select(c => c.職稱).Distinct().Where(t => t != null).ToListAsync(),
                searchDto.SearchTerm
            );

            return View(vm);
        }

        public async Task<IActionResult> DetailsAsync(int id)
        {
            var contact = await _unitOfWork.Contacts.GetByIdAsync(id);
            if (contact == null) return NotFound();
            var data = _mapper.Map<ContactViewModel>(contact);
            return View(data);
        }

        public async Task<IActionResult> Create()
        {
            ContactViewModel contact = new ContactViewModel();
            contact.dropdown = await Populate();
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] ContactViewModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = _mapper.Map<客戶聯絡人>(contact);
                    await _unitOfWork.Contacts.InsertAsync(data);
                    _unitOfWork.Save();
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"無法保存資料：{ex.InnerException?.Message ?? ex.Message}");
                }
            }
            return View(contact);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var contact = await _unitOfWork.Contacts.GetByIdAsync(id);
            if (contact == null) return NotFound();
            
            var data = _mapper.Map<ContactViewModel>(contact);

            data.dropdown = await Populate();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] ContactViewModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = _mapper.Map<客戶聯絡人>(contact);
                    await _unitOfWork.Contacts.GetByIdAsync(data);
                    _unitOfWork.Save();
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"無法保存資料：{ex.InnerException?.Message ?? ex.Message}");
                }
            }
            contact.dropdown = await Populate();
            return View(contact);
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            var contact = await _unitOfWork.Contacts.GetByIdAsync(id);
            if (contact == null) return NotFound();
            var data = _mapper.Map<ContactViewModel>(contact);
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAsync(int id)
        {
            try
            {
                await _unitOfWork.Contacts.SoftDeleteAsync(id);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"無法刪除資料：{ex.InnerException?.Message ?? ex.Message}");
                return RedirectToAction(nameof(DeleteAsync), new { id });
            }
        }

        public IActionResult Export(string cacheKey)
        {
            var contact = _cache.Get<IEnumerable<ContactViewModel>>(cacheKey);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("客戶聯絡人");
            worksheet.Cell(1, 1).Value = "職稱";
            worksheet.Cell(1, 2).Value = "姓名";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "手機";
            worksheet.Cell(1, 5).Value = "電話";

            int row = 2;
            foreach (var item in contact)
            {
                worksheet.Cell(row, 1).Value = item.職稱 ?? "";
                worksheet.Cell(row, 2).Value = item.姓名 ?? "";
                worksheet.Cell(row, 3).Value = item.Email ?? "";
                worksheet.Cell(row, 4).Value = item.手機 ?? "";
                worksheet.Cell(row, 5).Value = item.電話 ?? "";
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "客戶聯絡人.xlsx");
        }

        private async Task<ContactDropdown> Populate()
        {
            ContactDropdown dropdown = new();
            dropdown.客戶IdList = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱");

            return dropdown;
        }
    }
}