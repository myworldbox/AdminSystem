using ClosedXML.Excel;
using AdminSystem.Application.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using AdminSystem.Infrastructure.Repositories;

namespace AdminSystem.Web.Controllers
{
    public class BanksController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BanksController(IUnitOfWork unitOfWork = null)
        {
            _unitOfWork = unitOfWork;
        }

        public ActionResult Index(string search = "", string sort = "Id", string order = "asc")
        {
            Expression<Func<UserBankViewModel, bool>> filter = null;
            if (!string.IsNullOrEmpty(search))
            {
                filter = b => 
                b.銀行名稱.Contains(search) ||
                b.銀行代碼.ToString().Contains(search) ||
                b.分行代碼.ToString().Contains(search) ||
                b.帳戶名稱.Contains(search) ||
                b.帳戶號碼.ToString().Contains(search);
            }

            Func<IQueryable<UserBankViewModel>, IOrderedQueryable<UserBankViewModel>> orderBy = q => q.OrderBy(sort + " " + order);
            var banks = _unitOfWork.Banks.Get(filter, orderBy).ToList();

            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.Order = order == "asc" ? "desc" : "asc";
            ViewBag.Customers = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱");

            return View(banks);
        }

        public ActionResult Details(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
            if (bank == null) return NotFound();
            return View(bank);
        }

        public ActionResult Create()
        {
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,客戶Id,銀行名稱,銀行代碼,分行代碼,帳戶名稱,帳戶號碼")] UserBankViewModel bank)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Banks.Insert(bank);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", bank.客戶Id);
            return View(bank);
        }

        public ActionResult Edit(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
            if (bank == null) return NotFound();
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", bank.客戶Id);
            return View(bank);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,客戶Id,銀行名稱,銀行代碼,分行代碼,帳戶名稱,帳戶號碼")] UserBankViewModel bank)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Banks.Update(bank);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            ViewBag.客戶Id = new SelectList(_unitOfWork.Customers.Get(), "Id", "客戶名稱", bank.客戶Id);
            return View(bank);
        }

        public ActionResult Delete(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
            if (bank == null) return NotFound();
            return View(bank);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _unitOfWork.Banks.Delete(id);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public FileResult Export(string search = "")
        {
            Expression<Func<UserBankViewModel, bool>> filter = null;
            if (!string.IsNullOrEmpty(search))
            {
                filter = b => b.銀行名稱.Contains(search) || b.帳戶名稱.Contains(search);
            }

            var data = _unitOfWork.Banks.Get(filter).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("客戶銀行資訊");
                worksheet.Cell(1, 1).Value = "銀行名稱";
                worksheet.Cell(1, 2).Value = "銀行代碼";
                worksheet.Cell(1, 3).Value = "分行代碼";
                worksheet.Cell(1, 4).Value = "帳戶名稱";
                worksheet.Cell(1, 5).Value = "帳戶號碼";

                int row = 2;
                foreach (var item in data)
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