using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using System.Diagnostics.Contracts;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.Json;

namespace AdminSystem.Web.Controllers
{
    public class BankController(IUnitOfWork _unitOfWork, IMapper _mapper, IMemoryCache _cache) : Controller
    {
        public async Task<IActionResult> Index(SearchDto searchDto)
        {
            var query = _unitOfWork.Banks.Get();

            // 搜尋
            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                string term = searchDto.SearchTerm.ToUpper();
                query = query.Where(b =>
                    b.銀行名稱.Contains(term) ||
                    b.銀行代碼.ToString().Contains(term) ||
                    b.分行代碼.ToString().Contains(term) ||
                    b.帳戶名稱.Contains(term) ||
                    b.帳戶號碼.Contains(term));
            }

            // 排序
            string orderBy = searchDto.OrderName;
            if (searchDto.Order == Enums.Order.desc)
                orderBy += " descending";

            query = query.OrderBy(orderBy);

            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();

            var vm = new PagedResultDto<客戶銀行資訊>
            {
                Items = items,
                TotalRecords = totalRecords,
                SearchDto = searchDto
            };

            return View(vm);
        }

        public async Task<ActionResult> DetailsAsync(int id)
        {
            var bank = await _unitOfWork.Banks.GetByIdAsync(id);
            if (bank == null) return NotFound();
            var data = _mapper.Map<BankViewModel>(bank);
            return View(data);
        }

        [HttpGet]
        public async Task<ActionResult> Create()
        {
            BankViewModel bank = new();
            bank.dropdown = await Populate();
            return View(bank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Id,客戶Id,銀行名稱,銀行代碼,分行代碼,帳戶名稱,帳戶號碼")] BankViewModel bank)
        {
            if (ModelState.IsValid)
            {
                var data = _mapper.Map<客戶銀行資訊>(bank);
                await _unitOfWork.Banks.GetByIdAsync(data);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            bank.dropdown = await Populate();
            return View(bank);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var bank = await _unitOfWork.Banks.GetByIdAsync(id);
            if (bank == null) return NotFound();
            var data = _mapper.Map<BankViewModel>(bank);
            data.dropdown = await Populate();
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind("Id,客戶Id,銀行名稱,銀行代碼,分行代碼,帳戶名稱,帳戶號碼")] BankViewModel bank)
        {
            if (ModelState.IsValid)
            {
                var data = _mapper.Map<客戶銀行資訊>(bank);
                await _unitOfWork.Banks.GetByIdAsync(data);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(bank);
        }

        public async Task<ActionResult> DeleteAsync(int id)
        {
            var bank = await _unitOfWork.Banks.GetByIdAsync(id);
            if (bank == null) return NotFound();
            var data = _mapper.Map<BankViewModel>(bank);
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync(int id)
        {
            await _unitOfWork.Banks.SoftDeleteAsync(id);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public FileResult Export(string cacheKey)
        {
            var bank = _cache.Get<IEnumerable<BankViewModel>>(cacheKey);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("客戶銀行資訊");
                worksheet.Cell(1, 1).Value = "銀行名稱";
                worksheet.Cell(1, 2).Value = "銀行代碼";
                worksheet.Cell(1, 3).Value = "分行代碼";
                worksheet.Cell(1, 4).Value = "帳戶名稱";
                worksheet.Cell(1, 5).Value = "帳戶號碼";

                int row = 2;
                foreach (var item in bank)
                {
                    worksheet.Cell(row, 1).Value = item.銀行名稱;
                    worksheet.Cell(row, 2).Value = item.銀行代碼;
                    worksheet.Cell(row, 3).Value = item.分行代碼;
                    worksheet.Cell(row, 4).Value = item.帳戶名稱;
                    worksheet.Cell(row, 5).Value = item.帳戶號碼;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "客戶銀行資訊.xlsx");
                }
            }
        }

        private async Task<BankDropdown> Populate()
        {
            BankDropdown dropdown = new();
            dropdown.客戶IdList = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱");

            return dropdown;
        }
    }
}