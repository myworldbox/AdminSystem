using ClosedXML.Excel;
using AdminSystem.Models;
using AdminSystem.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using AdminSystem.Repositories;

namespace AdminSystem.Controllers
{
    public class ContactsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContactsController(IUnitOfWork unitOfWork = null)
        {
            _unitOfWork = unitOfWork;
        }

        public ActionResult Index(string search = "", string jobTitle = "", string sort = "姓名", string order = "asc")
        {
            Expression<Func<客戶聯絡人, bool>> filter = null;
            if (!string.IsNullOrEmpty(search))
            {
                filter = c => c.姓名.Contains(search) || c.Email.Contains(search) || c.職稱.Contains(search);
            }
            if (!string.IsNullOrEmpty(jobTitle) && jobTitle != "全部")
            {
                Expression<Func<客戶聯絡人, bool>> titleFilter = c => c.職稱 == jobTitle;
                filter = filter == null ? titleFilter : (c => filter.Compile()(c) && titleFilter.Compile()(c));
            }

            Func<IQueryable<客戶聯絡人>, IOrderedQueryable<客戶聯絡人>> orderBy = q => q.OrderBy(sort + " " + order);
            var contacts = _unitOfWork.Contacts.Get(filter, orderBy).ToList();

            ViewBag.Search = search;
            ViewBag.JobTitle = jobTitle;
            ViewBag.Sort = sort;
            ViewBag.Order = order == "asc" ? "desc" : "asc";
            ViewBag.JobTitles = new SelectList(_unitOfWork.Contacts.Get().Select(c => c.職稱).Distinct().ToList(), jobTitle);
            ViewBag.Customers = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱");

            return View(contacts);
        }

        public ActionResult Details(int id)
        {
            var contact = _unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            return View(contact);
        }

        public ActionResult Create()
        {
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] 客戶聯絡人 contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Contacts.Insert(contact);
                    _unitOfWork.Save();
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var ve in ex.EntityValidationErrors)
                    {
                        foreach (var vee in ve.ValidationErrors)
                        {
                            ModelState.AddModelError(vee.PropertyName, vee.ErrorMessage);
                        }
                    }
                }
            }
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        public ActionResult Edit(int id)
        {
            var contact = _unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] 客戶聯絡人 contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.Contacts.Update(contact);
                    _unitOfWork.Save();
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var ve in ex.EntityValidationErrors)
                    {
                        foreach (var vee in ve.ValidationErrors)
                        {
                            ModelState.AddModelError(vee.PropertyName, vee.ErrorMessage);
                        }
                    }
                }
            }
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        public ActionResult Delete(int id)
        {
            var contact = _unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            return View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _unitOfWork.Contacts.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public FileResult Export(string search = "", string jobTitle = "")
        {
            Expression<Func<客戶聯絡人, bool>> filter = null;
            if (!string.IsNullOrEmpty(search))
            {
                filter = c => c.姓名.Contains(search) || c.Email.Contains(search) || c.職稱.Contains(search);
            }
            if (!string.IsNullOrEmpty(jobTitle) && jobTitle != "全部")
            {
                Expression<Func<客戶聯絡人, bool>> titleFilter = c => c.職稱 == jobTitle;
                filter = filter == null ? titleFilter : (c => filter.Compile()(c) && titleFilter.Compile()(c));
            }

            var data = _unitOfWork.Contacts.Get(filter).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("客戶聯絡人");
                worksheet.Cell(1, 1).Value = "職稱";
                worksheet.Cell(1, 2).Value = "姓名";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "手機";
                worksheet.Cell(1, 5).Value = "電話";

                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.職稱;
                    worksheet.Cell(row, 2).Value = item.姓名;
                    worksheet.Cell(row, 3).Value = item.Email;
                    worksheet.Cell(row, 4).Value = item.手機;
                    worksheet.Cell(row, 5).Value = item.電話;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "客戶聯絡人.xlsx");
                }
            }
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