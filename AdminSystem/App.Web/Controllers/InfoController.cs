using AdminSystem.Application.Dtos;
using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace AdminSystem.Web.Controllers
{
    public class InfoController(IUnitOfWork _unitOfWork, IMapper _mapper, IMemoryCache _cache) : Controller
    {
        public async Task<IActionResult> Index(SearchDto searchDto)
        {
            // start with base query
            var query = _unitOfWork.Infos.Get();

            if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            {
                string SearchTerm = searchDto.SearchTerm.ToUpper();
                query = query.Where(c =>
                    c.客戶名稱.Contains(SearchTerm) ||
                    c.統一編號.Contains(SearchTerm) ||
                    c.電話.Contains(SearchTerm) ||
                    c.地址.Contains(SearchTerm) ||
                    c.Email.Contains(SearchTerm));
            }

            // Sorting
            query = searchDto.Order switch
            {
                Enums.Order.desc => query.OrderByDescending(s => s.Id),
                Enums.Order.asc => query.OrderBy(s => s.Id),
                _ => query.OrderBy(s => s.Id)
            };

            // Total count (important for pagination)
            var totalRecords = await query.CountAsync();

            // Pagination
            var items = await query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(s => s)
                .ToListAsync();

            var vm = new PagedResultDto<客戶資料>
            {
                Items = items,
                TotalRecords = totalRecords,
                CurrentPage = searchDto.Page,
                PageSize = searchDto.PageSize
            };

            return View(vm);
        }

        public IActionResult Details(int id)
        {
            var info = _unitOfWork.Infos.GetById(id);
            if (info == null) return NotFound();
            var data = _mapper.Map<InfoViewModel>(info);
            return View(data);
        }

        public IActionResult Create()
        {
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,客戶名稱,統一編號,電話,傳真,地址,Email,客戶分類")] InfoViewModel info)
        {
            if (ModelState.IsValid)
            {
                var data = _mapper.Map<客戶資料>(info);
                _unitOfWork.Infos.Insert(data);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)));
            return View(info);
        }

        public IActionResult Edit(int id)
        {
            var info = _unitOfWork.Infos.GetById(id);
            if (info == null) return NotFound();
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)), info.客戶分類);
            var data = _mapper.Map<InfoViewModel>(info);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("Id,客戶名稱,統一編號,電話,傳真,地址,Email,客戶分類")] InfoViewModel info)
        {
            if (ModelState.IsValid)
            {
                var data = _mapper.Map<客戶資料>(info);
                _unitOfWork.Infos.Update(data);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)), info.客戶分類);
            return View(info);
        }

        public IActionResult Delete(int id)
        {
            var info = _unitOfWork.Infos.GetById(id);
            if (info == null) return NotFound();
            var data = _mapper.Map<InfoViewModel>(info);
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _unitOfWork.Infos.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public FileResult Export(string cacheKey)
        {
            var info = _cache.Get<IEnumerable<InfoViewModel>>(cacheKey);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("客戶資料");
            worksheet.Cell(1, 1).Value = "客戶名稱";
            worksheet.Cell(1, 2).Value = "統一編號";
            worksheet.Cell(1, 3).Value = "電話";
            worksheet.Cell(1, 4).Value = "傳真";
            worksheet.Cell(1, 5).Value = "地址";
            worksheet.Cell(1, 6).Value = "Email";
            worksheet.Cell(1, 7).Value = "客戶分類";

            int row = 2;
            foreach (var item in info)
            {
                worksheet.Cell(row, 1).Value = item.客戶名稱;
                worksheet.Cell(row, 2).Value = item.統一編號;
                worksheet.Cell(row, 3).Value = item.電話;
                worksheet.Cell(row, 4).Value = item.傳真;
                worksheet.Cell(row, 5).Value = item.地址;
                worksheet.Cell(row, 6).Value = item.Email;
                worksheet.Cell(row, 7).Value = item.客戶分類.ToString();
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "客戶資料.xlsx");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}