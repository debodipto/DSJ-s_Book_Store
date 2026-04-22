using DSJsBookStore.Constants;
using Microsoft.AspNetCore.Mvc;
using DSJsBookStore.Repositories;
using DSJsBookStore.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace DSJsBookStore.Controllers;

[Authorize(Roles = nameof(Roles.Admin))]
public class ReportsController : Controller
{
    private readonly IReportRepository _reportRepository;

    public ReportsController(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IActionResult> TopFiveSellingBooks(DateTime? sDate = null, DateTime? eDate = null)
    {
        try
        {
            DateTime startDate = sDate ?? DateTime.UtcNow.AddDays(-7);
            DateTime endDate = eDate ?? DateTime.UtcNow;

            var topBooks = await _reportRepository.GetTopNSellingBooksByDate(startDate, endDate);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(topBooks);
        }
        catch
        {
            TempData["errorMessage"] = "Something went wrong";
            return RedirectToAction("Index", "Home");
        }
    }
}
