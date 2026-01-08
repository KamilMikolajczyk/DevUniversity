using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Data;
using University.Models;

namespace University.Controllers;

[Authorize(Roles = "Admin")]
public class AdminSubjectsController : Controller
{
    private readonly UnivercityDbContext _db;

    public AdminSubjectsController(UnivercityDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.Subjects
            .Include(s => s.Lecturer)
            .ThenInclude(l => l.User)
            .OrderBy(s => s.Faculty)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Lecturers = await _db.Lecturers
            .Include(l => l.User)
            .OrderBy(l => l.User.Email)
            .ToListAsync();

        return View(new Subject());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Subject model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Lecturers = await _db.Lecturers.Include(l => l.User).OrderBy(l => l.User.Email).ToListAsync();
            return View(model);
        }

        _db.Subjects.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastOk"] = "Dodano przedmiot.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        ViewBag.Lecturers = await _db.Lecturers
            .Include(l => l.User)
            .OrderBy(l => l.User.Email)
            .ToListAsync();

        return View(subject);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Subject model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Lecturers = await _db.Lecturers.Include(l => l.User).OrderBy(l => l.User.Email).ToListAsync();
            return View(model);
        }

        var subject = await _db.Subjects.FindAsync(id);
        if (subject == null) return NotFound();

        subject.Name = model.Name;
        subject.Faculty = model.Faculty;
        subject.LecturerId = model.LecturerId;

        await _db.SaveChangesAsync();

        TempData["ToastOk"] = "Zapisano zmiany.";
        return RedirectToAction(nameof(Index));
    }
}
