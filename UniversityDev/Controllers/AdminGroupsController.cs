using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Data;
using University.Models;

namespace University.Controllers;

[Authorize(Roles = "Admin")]
public class AdminGroupsController : Controller
{
    private readonly UnivercityDbContext _db;
    public AdminGroupsController(UnivercityDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var groups = await _db.StudentGroups
            .Include(g => g.Students)
            .OrderBy(g => g.Code)
            .ToListAsync();
        return View(groups);
    }

    public IActionResult Create() => View(new StudentGroup());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentGroup model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Code = model.Code.Trim();
        if (await _db.StudentGroups.AnyAsync(g => g.Code == model.Code))
        {
            ModelState.AddModelError(nameof(model.Code), "Taka grupa już istnieje.");
            return View(model);
        }

        _db.StudentGroups.Add(model);
        await _db.SaveChangesAsync();
        TempData["ToastOk"] = "Utworzono grupę.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ManageStudents(int id)
    {
        var group = await _db.StudentGroups.FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return NotFound();

        ViewBag.Group = group;
        ViewBag.Groups = await _db.StudentGroups.OrderBy(g => g.Code).ToListAsync();

        var students = await _db.Students
            .Include(s => s.User)
            .OrderBy(s => s.IndexNumber)
            .ToListAsync();

        return View(students);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAssign(int groupId, int returnGroupId, int[] studentIds)
    {
        if (studentIds == null || studentIds.Length == 0)
        {
            TempData["ToastError"] = "Nie wybrano żadnych studentów.";
            return RedirectToAction(nameof(ManageStudents), new { id = returnGroupId });
        }

        if (!await _db.StudentGroups.AnyAsync(g => g.Id == groupId))
        {
            TempData["ToastError"] = "Wybrana grupa nie istnieje.";
            return RedirectToAction(nameof(ManageStudents), new { id = returnGroupId });
        }

        var students = await _db.Students.Where(s => studentIds.Contains(s.Id)).ToListAsync();
        foreach (var s in students)
            s.StudentGroupId = groupId;

        await _db.SaveChangesAsync();
        TempData["ToastOk"] = $"Przypisano {students.Count} studentów do grupy.";
        return RedirectToAction(nameof(ManageStudents), new { id = returnGroupId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStudentGroup(int studentId, int? groupId, int returnGroupId)
    {
        var student = await _db.Students.FirstOrDefaultAsync(s => s.Id == studentId);
        if (student == null) return NotFound();

        if (groupId.HasValue && !await _db.StudentGroups.AnyAsync(g => g.Id == groupId.Value))
        {
            TempData["ToastError"] = "Wybrana grupa nie istnieje.";
            return RedirectToAction(nameof(ManageStudents), new { id = returnGroupId });
        }

        student.StudentGroupId = groupId;
        await _db.SaveChangesAsync();
        TempData["ToastOk"] = "Zapisano zmianę grupy studenta.";
        return RedirectToAction(nameof(ManageStudents), new { id = returnGroupId });
    }
}
