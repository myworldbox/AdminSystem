using AdminSystem.Application.Dtos;
using AdminSystem.Application.Services;
using AdminSystem.Application.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace AdminSystem.Web.Controllers;

public class InfoController : Controller
{
    private readonly IInfoService _infoService;

    public InfoController(IInfoService infoService)
    {
        _infoService = infoService;
    }

    public async Task<IActionResult> Index(SearchDto searchDto)
    {
        var result = await _infoService.GetPagedAsync(searchDto);

        ViewData["Title"] = "客戶資料管理";

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var vm = await _infoService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    public async Task<IActionResult> Create()
        => View(await _infoService.GetForCreateAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InfoViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _infoService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        model.dropdown = await _infoService.PopulateDropdownAsync();
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
        => View(await _infoService.GetForEditAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(InfoViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _infoService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        model.dropdown = await _infoService.PopulateDropdownAsync();
        return View(model);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _infoService.GetByIdAsync(id);
        if (vm == null) return NotFound();
        return View(vm);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _infoService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Export(SearchDto searchDto)
    {
        var data = _infoService.GetAllForExport(searchDto);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("客戶資料");

        // Header
        var headers = new[] { "客戶名稱", "統一編號", "電話", "傳真", "地址", "Email", "客戶分類" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).SetValue(headers[i]).Style.Font.SetBold();

        // Data (streaming)
        int row = 2;
        await foreach (var item in data)
        {
            ws.Cell(row, 1).SetValue(item.客戶名稱);
            ws.Cell(row, 2).SetValue(item.統一編號);
            ws.Cell(row, 3).SetValue(item.電話);
            ws.Cell(row, 4).SetValue(item.傳真);
            ws.Cell(row, 5).SetValue(item.地址);
            ws.Cell(row, 6).SetValue(item.Email);
            ws.Cell(row, 7).SetValue(item.客戶分類);
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);

        var fileName = $"客戶資料_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}