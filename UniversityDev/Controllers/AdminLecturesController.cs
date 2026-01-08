using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Data;
using University.Models;

namespace University.Controllers;

[Authorize(Roles = "Admin")]
public class AdminLecturesController : Controller
{
    private readonly UnivercityDbContext _db;

    public AdminLecturesController(UnivercityDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _db.CalendarEvents
            .Include(e => e.Subject)
            .Where(e => e.IsLecture)
            .OrderByDescending(e => e.StartUtc)
            .Take(300)
            .ToListAsync();

        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Subjects = await _db.Subjects
            .OrderBy(s => s.Name)
            .ToListAsync();

        // defaults
        var now = DateTime.UtcNow;
        return View(new CalendarEvent
        {
            Title = "Zajęcia",
            StartUtc = now,
            EndUtc = now.AddHours(1),
            Visibility = CalendarVisibility.OnlyEnrolled,
            IsLecture = true
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CalendarEvent model)
    {
        ViewBag.Subjects = await _db.Subjects.OrderBy(s => s.Name).ToListAsync();

        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError(nameof(model.Title), "Tytuł jest wymagany.");

        if (model.EndUtc <= model.StartUtc)
            ModelState.AddModelError(nameof(model.EndUtc), "Koniec musi być po początku.");

        if (!ModelState.IsValid) return View(model);

        model.Title = model.Title.Trim();
        model.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim();
        model.IsLecture = true;

        // ensure UTC kind
        model.StartUtc = DateTime.SpecifyKind(model.StartUtc, DateTimeKind.Utc);
        model.EndUtc = DateTime.SpecifyKind(model.EndUtc, DateTimeKind.Utc);

        _db.CalendarEvents.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastOk"] = "Dodano zajęcia.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.IsLecture);
        if (ev == null) return NotFound();

        _db.CalendarEvents.Remove(ev);
        await _db.SaveChangesAsync();
        TempData["ToastOk"] = "Usunięto zajęcia.";
        return RedirectToAction(nameof(Index));
    }
}
