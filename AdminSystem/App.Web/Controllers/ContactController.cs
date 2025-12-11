using AdminSystem.Application.Dtos;
using AdminSystem.Application.Services;
using AdminSystem.Application.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Web.Controllers;

public class ContactController : Controller
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    public async Task<IActionResult> Index(SearchDto searchDto)
    {
        var result = await _contactService.GetPagedAsync(searchDto);

        ViewData["Title"] = "客戶聯絡人管理";

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _contactService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Create()
        => View(await _contactService.GetForCreateAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _contactService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        model.dropdown = await _contactService.PopulateDropdownAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
        => View(await _contactService.GetForEditAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ContactViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _contactService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        model.dropdown = await _contactService.PopulateDropdownAsync();
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _contactService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _contactService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(SearchDto searchDto)
    {
        var data = _contactService.GetAllForExport(searchDto);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("客戶聯絡人");

        // Header
        var headers = new[] { "客戶名稱", "職稱", "姓名", "Email", "手機", "電話" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).SetValue(headers[i]).Style.Font.SetBold();

        // Data - Streaming
        int row = 2;
        await foreach (var item in data)
        {
            // 建議你在 ContactService.GetAllForExport 裡就 Join 客戶名稱，下面示範兩種做法
            ws.Cell(row, 1).SetValue(item.客戶?.客戶名稱 ?? ""); // 若有 Include
            ws.Cell(row, 2).SetValue(item.職稱);
            ws.Cell(row, 3).SetValue(item.姓名);
            ws.Cell(row, 4).SetValue(item.Email);
            ws.Cell(row, 5).SetValue(item.手機);
            ws.Cell(row, 6).SetValue(item.電話);
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var fileName = $"客戶聯絡人_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}