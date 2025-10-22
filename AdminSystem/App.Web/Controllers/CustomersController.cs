using ClosedXML.Excel;
using AdminSystem.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Dynamic.Core;
using AdminSystem.Infrastructure.Repositories;
using AdminSystem.Models;

namespace AdminSystem.Web.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string search = "", string category = "", string sort = "Id", string order = "asc")
        {
            // start with base query
            var query = _unitOfWork.Customers.Get();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.客戶名稱.Contains(search) ||
                    c.統一編號.Contains(search) ||
                    c.電話.Contains(search) ||
                    c.地址.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.客戶分類.Contains(search));
            }

            if (!string.IsNullOrEmpty(category) && category != "全部")
            {
                query = query.Where(c => c.客戶分類 == category);
            }

            // dynamic sort (requires System.Linq.Dynamic.Core)
            query = query.OrderBy($"{sort} {order}");

            var customers = query.ToList();   // should now work

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Sort = sort;
            ViewBag.Order = order == "asc" ? "desc" : "asc";
            ViewBag.Categories = new SelectList(Enum.GetValues(typeof(Enums.Category)).Cast<Enums.Category>().Select(e => e.ToString()), category);
            ViewData["Title"] = "Customers";

            return View(customers);
        }

        public IActionResult Details(int id)
        {
            var customer = _unitOfWork.Customers.GetById(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        public IActionResult Create()
        {
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,客戶名稱,統一編號,電話,傳真,地址,Email,客戶分類")] InfoViewModel customer)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Customers.Insert(customer);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)));
            return View(customer);
        }

        public IActionResult Edit(int id)
        {
            var customer = _unitOfWork.Customers.GetById(id);
            if (customer == null) return NotFound();
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)), customer.客戶分類);
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("Id,客戶名稱,統一編號,電話,傳真,地址,Email,客戶分類")] InfoViewModel customer)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Customers.Update(customer);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.客戶分類 = new SelectList(Enum.GetValues(typeof(Enums.Category)), customer.客戶分類);
            return View(customer);
        }

        public IActionResult Delete(int id)
        {
            var customer = _unitOfWork.Customers.GetById(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _unitOfWork.Customers.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public FileResult Export(string search = "", string category = "")
        {
            var query = _unitOfWork.Customers.Get();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.客戶名稱.Contains(search) ||
                    c.統一編號.Contains(search) ||
                    c.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(category) && category != "全部")
            {
                query = query.Where(c => c.客戶分類 == category);
            }

            var data = query.ToList();

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
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.客戶名稱;
                worksheet.Cell(row, 2).Value = item.統一編號;
                worksheet.Cell(row, 3).Value = item.電話;
                worksheet.Cell(row, 4).Value = item.傳真;
                worksheet.Cell(row, 5).Value = item.地址;
                worksheet.Cell(row, 6).Value = item.Email;
                worksheet.Cell(row, 7).Value = item.客戶分類;
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