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
        public ActionResult Index(string search = "", string sort = "Id", Enums.Order order = Enums.Order.asc)
        {
            Expression<Func<客戶銀行資訊, bool>>? filter = null;
            if (!string.IsNullOrEmpty(search))
            {
                filter = b => 
                b.銀行名稱.Contains(search) ||
                b.銀行代碼.ToString().Contains(search) ||
                b.分行代碼.ToString().Contains(search) ||
                b.帳戶名稱.Contains(search) ||
                b.帳戶號碼.ToString().Contains(search);
            }

            Func<IQueryable<客戶銀行資訊>, IOrderedQueryable<客戶銀行資訊>> orderBy = q => q.OrderBy(sort + " " + order);
            var banks = _unitOfWork.Banks.Get(filter, orderBy).ToList();

            var data = _mapper.Map<IEnumerable<BankViewModel>>(banks);
            var cacheKey = Guid.NewGuid().ToString();

            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(10));

            if (ViewBag.CacheKey != null)
            {
                _cache.Remove(ViewBag.CacheKey);
            }

            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.Order = ((int)order + 1) % Enum.GetValues<Enums.Order>().Length;
            ViewBag.Customers = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱");
            ViewBag.CacheKey = cacheKey;
            ViewData["Title"] = "Bank";

            return View(data);
        }

        public ActionResult Details(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
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
                _unitOfWork.Banks.Insert(data);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            bank.dropdown = await Populate();
            return View(bank);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
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
                _unitOfWork.Banks.Update(data);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(bank);
        }

        public ActionResult Delete(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
            if (bank == null) return NotFound();
            var data = _mapper.Map<BankViewModel>(bank);
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _unitOfWork.Banks.Delete(id);
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