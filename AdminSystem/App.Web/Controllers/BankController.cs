using ClosedXML.Excel;
using AdminSystem.Application.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using AdminSystem.Infrastructure.Repositories;
using AdminSystem.Domain;
using AutoMapper;
using AdminSystem.Domain.Entities;
using System.Diagnostics.Contracts;

namespace AdminSystem.Web.Controllers
{
    public class BankController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BankController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

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

            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.Order = ((int)order + 1) % Enum.GetValues<Enums.Order>().Length;
            ViewBag.Customers = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱");
            ViewData["Title"] = "Banks";

            var data = _mapper.Map<IEnumerable<BankViewModel>>(banks);

            return View(data);
        }

        public ActionResult Details(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
            if (bank == null) return NotFound();
            var data = _mapper.Map<BankViewModel>(bank);
            return View(data);
        }

        public ActionResult Create()
        {
            ViewBag.客戶Id = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,客戶Id,銀行名稱,銀行代碼,分行代碼,帳戶名稱,帳戶號碼")] BankViewModel bank)
        {
            if (ModelState.IsValid)
            {
                var data = _mapper.Map<客戶銀行資訊>(bank);
                _unitOfWork.Banks.Insert(data);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            ViewBag.客戶Id = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱", bank.客戶Id);
            return View(bank);
        }

        public ActionResult Edit(int id)
        {
            var bank = _unitOfWork.Banks.GetById(id);
            if (bank == null) return NotFound();
            ViewBag.客戶Id = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱", bank.客戶id);
            var data = _mapper.Map<BankViewModel>(bank);
            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,客戶Id,銀行名稱,銀行代碼,分行代碼,帳戶名稱,帳戶號碼")] BankViewModel bank)
        {
            if (ModelState.IsValid)
            {
                var data = _mapper.Map<客戶銀行資訊>(bank);
                _unitOfWork.Banks.Update(data);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            ViewBag.客戶Id = new SelectList(_unitOfWork.Infos.Get(), "Id", "客戶名稱", bank.客戶Id);
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

        public FileResult Export(string search = "", string sort = "Id", Enums.Order order = Enums.Order.asc)
        {
            Expression<Func<客戶銀行資訊, bool>> filter = null;
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
            var data = _unitOfWork.Banks.Get(filter, orderBy).ToList();

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