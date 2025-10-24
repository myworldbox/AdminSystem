using AdminSystem.Application.ViewModels;
using AdminSystem.Domain;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Repositories;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace AdminSystem.Web.Controllers
{
    public class InfoController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;

        public InfoController(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cache = cache;
        }

        public IActionResult Index(string search = "", string sort = "Id", Enums.Order order = Enums.Order.asc, Enums.Category? category = null)
        {
            // start with base query
            var query = _unitOfWork.Infos.Get();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.客戶名稱.Contains(search) ||
                    c.統一編號.Contains(search) ||
                    c.電話.Contains(search) ||
                    c.地址.Contains(search) ||
                    c.Email.Contains(search));
            }

            if (category != null)
            {
                query = query.Where(c => c.客戶分類 == category.ToString());
            }

            // dynamic sort (requires System.Linq.Dynamic.Core)
            query = query.OrderBy($"{sort} {order}");

           var infos = query.ToList();

            var data = _mapper.Map<IEnumerable<InfoViewModel>>(infos);
            var cacheKey = Guid.NewGuid().ToString();

            _cache.Set(cacheKey, data, TimeSpan.FromMinutes(10));

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Sort = sort;
            ViewBag.Order = ((int)order + 1) % Enum.GetValues<Enums.Order>().Length;
            ViewBag.Categories = new SelectList(
                Enum.GetValues(typeof(Enums.Category)).Cast<Enums.Category>().Select(e => new
                {
                    Value = e,
                    Text = e.ToString()
                }),
                "Value",
                "Text",
                category
            );
            ViewBag.CacheKey = cacheKey;
            ViewData["Title"] = "Info";

            return View(data);
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