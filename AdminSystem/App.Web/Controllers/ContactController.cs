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

namespace AdminSystem.Web.Controllers
{
    public class ContactController(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache) : Controller
    {
        public IActionResult Index(string search = "", string jobTitle = "", string sort = "Id", Enums.Order order = Enums.Order.asc)
        {
            var query = unitOfWork.Contacts.Get();

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

            var data = mapper.Map<IEnumerable<ContactViewModel>>(contacts);

            var cacheKey = Guid.NewGuid().ToString();

            cache.Set(cacheKey, data, TimeSpan.FromMinutes(10));

            if (ViewBag.CacheKey != null)
            {
                cache.Remove(ViewBag.CacheKey);
            }

            ViewBag.Search = search;
            ViewBag.JobTitle = jobTitle;
            ViewBag.Sort = sort;
            ViewBag.Order = ((int)order + 1) % Enum.GetValues<Enums.Order>().Length;
            ViewBag.JobTitles = new SelectList(unitOfWork.Contacts.Get().Select(c => c.職稱).Distinct().ToList(), jobTitle);
            ViewBag.Customers = new SelectList(unitOfWork.Infos.Get(), "Id", "客戶名稱");
            ViewBag.CacheKey = cacheKey;
            ViewData["Title"] = "Contact";

            return View(data);
        }

        public IActionResult Details(int id)
        {
            var contact = unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            var data = mapper.Map<ContactViewModel>(contact);
            return View(data);
        }

        public IActionResult Create()
        {
            ViewBag.客戶Id = new SelectList(unitOfWork.Infos.Get(), "Id", "客戶名稱");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] ContactViewModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = mapper.Map<客戶聯絡人>(contact);
                    unitOfWork.Contacts.Insert(data);
                    unitOfWork.Save();
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
            ViewBag.客戶Id = new SelectList(unitOfWork.Infos.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        public IActionResult Edit(int id)
        {
            var contact = unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            ViewBag.客戶Id = new SelectList(unitOfWork.Infos.Get(), "Id", "客戶名稱", contact.客戶Id);
            var data = mapper.Map<ContactViewModel>(contact);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("Id,客戶Id,職稱,姓名,Email,手機,電話")] ContactViewModel contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var data = mapper.Map<客戶聯絡人>(contact);
                    unitOfWork.Contacts.Update(data);
                    unitOfWork.Save();
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
            ViewBag.客戶Id = new SelectList(unitOfWork.Infos.Get(), "Id", "客戶名稱", contact.客戶Id);
            return View(contact);
        }

        public IActionResult Delete(int id)
        {
            var contact = unitOfWork.Contacts.GetById(id);
            if (contact == null) return NotFound();
            var data = mapper.Map<ContactViewModel>(contact);
            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                unitOfWork.Contacts.Delete(id);
                unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"無法刪除資料：{ex.InnerException?.Message ?? ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        public IActionResult Export(string cacheKey)
        {
            var contact = cache.Get<IEnumerable<ContactViewModel>>(cacheKey);

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}