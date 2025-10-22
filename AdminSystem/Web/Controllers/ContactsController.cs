using ClosedXML.Excel;
using AdminSystem.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using AdminSystem.Infrastructure.Repositories;

namespace AdminSystem.Web.Controllers
{
    public class ContactsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContactsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string search = "", string jobTitle = "", string sort = "Id", string order = "asc")
        {
            var query = _unitOfWork.Contacts.Get();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c =>
                    c.姓名.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.職稱.Contains(search) ||
                    c.手機 != null && c.手機.Contains(search) ||
                    c.電話 != null && c.電話.Contains(search));
            }

            if (!string.IsNullOrEmpty(jobTitle) && jobTitle != "全部")
            {
                query = query.Where(c => c.職稱 == jobTitle);
            }

            // dynamic sort (requires System.Linq.Dynamic.Core)
            query = query.OrderBy($"{sort} {order}");

            var contacts = query.ToList();

            ViewBag.Search = search;
            ViewBag.JobTitle = jobTitle;
            ViewBag.Sort = sort;
            ViewBag.Order = order == "asc" ? "desc" : "asc";
            ViewBag.JobTitles = new SelectList(_unitOfWork.Contacts.Get().Select(c => c.職稱).Distinct().ToList(), jobTitle);
            ViewBag.Customers = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱");

            return View(contacts);
        }

        public IActionResult Details(int id)
        {
            var contact = _unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            return View(contact);
        }

        public IActionResult Create()
        {
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] UserContactViewModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Contacts.Insert(contact);
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
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        public IActionResult Edit(int id)
        {
            var contact = _unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] UserContactViewModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Contacts.Update(contact);
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
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        public IActionResult Delete(int id)
        {
            var contact = _unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            return View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _unitOfWork.Contacts.Delete(id);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"無法刪除資料：{ex.InnerException?.Message ?? ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        public IActionResult Export(string search = "", string jobTitle = "")
        {
            var query = _unitOfWork.Contacts.Get();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.姓名.Contains(search) ||
                                        c.Email.Contains(search) ||
                                        c.職稱.Contains(search) ||
                                        c.手機 != null && c.手機.Contains(search) ||
                                        c.電話 != null && c.電話.Contains(search));
            }

            if (!string.IsNullOrEmpty(jobTitle) && jobTitle != "全部")
            {
                query = query.Where(c => c.職稱 == jobTitle);
            }

            var data = query.ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("客戶聯絡人");
            worksheet.Cell(1, 1).Value = "職稱";
            worksheet.Cell(1, 2).Value = "姓名";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "手機";
            worksheet.Cell(1, 5).Value = "電話";

            int row = 2;
            foreach (var item in data)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}