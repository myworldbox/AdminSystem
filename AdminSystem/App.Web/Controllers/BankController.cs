using AdminSystem.Application.Dtos;
using AdminSystem.Application.Services;
using AdminSystem.Application.ViewModels;
using AdminSystem.Web.Controllers;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

public class BankController : Controller
{
    private readonly IBankService _bankService;

    public BankController(IBankService bankService)
    {
        _bankService = bankService;
    }

    public async Task<IActionResult> Index(SearchDto searchDto)
    {
        var result = await _bankService.GetPagedAsync(searchDto);

        ViewData["Title"] = "客戶銀行帳戶管理";

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _bankService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Create()
        => View(await _bankService.GetForCreateAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BankViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _bankService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        model.dropdown = await _bankService.PopulateDropdownAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
        => View(await _bankService.GetForEditAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BankViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _bankService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        model.dropdown = await _bankService.PopulateDropdownAsync();
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _bankService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _bankService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(SearchDto searchDto)
    {
        var data = _bankService.GetAllForExport(searchDto);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("客戶銀行資訊");

        var headers = new[] { "客戶名稱", "銀行名稱", "銀行代碼", "分行代碼", "帳戶名稱", "帳戶號碼" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).SetValue(headers[i]).Style.Font.SetBold();

        int row = 2;
        await foreach (var item in data)
        {
            ws.Cell(row, 1).SetValue(item.客戶?.客戶名稱 ?? "");
            ws.Cell(row, 2).SetValue(item.銀行名稱);
            ws.Cell(row, 3).SetValue(item.銀行代碼);
            ws.Cell(row, 4).SetValue(item.分行代碼);
            ws.Cell(row, 5).SetValue(item.帳戶名稱);
            ws.Cell(row, 6).SetValue(item.帳戶號碼);
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var fileName = $"客戶銀行資訊_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}