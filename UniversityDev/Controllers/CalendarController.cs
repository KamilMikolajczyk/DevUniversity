using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Data;
using University.Models;

namespace University.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly UnivercityDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CalendarController(UnivercityDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var student = await _db.Students
            .Include(s => s.StudentSubjects)
            .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

        var lecturer = await _db.Lecturers
            .FirstOrDefaultAsync(l => l.ApplicationUserId == user.Id);

        var enrolled = student?.StudentSubjects.Select(ss => ss.SubjectId).ToHashSet()
                      ?? new HashSet<int>();

        var myFaculty = student?.Faculty ?? lecturer?.Faculty;

        var events = await _db.CalendarEvents
            .Include(e => e.Subject)
            .Where(e =>
                (e.Visibility == CalendarVisibility.Private && e.OwnerUserId == user.Id)
                || (e.Visibility == CalendarVisibility.PublicToFaculty && e.Faculty != null && myFaculty != null && e.Faculty == myFaculty)
                || (e.Visibility == CalendarVisibility.OnlyEnrolled && e.SubjectId != null && enrolled.Contains(e.SubjectId.Value))
                || (e.Visibility == CalendarVisibility.OnlyLecturer && lecturer != null && e.Subject != null && e.Subject.LecturerId == lecturer.Id)
            )
            .OrderBy(e => e.StartUtc)
            .Take(300)
            .ToListAsync();

        return View(events);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePrivate(string title, DateTime startUtc, DateTime endUtc, string? location)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (string.IsNullOrWhiteSpace(title) || endUtc <= startUtc)
        {
            TempData["ToastError"] = "Niepoprawne dane wydarzenia.";
            return RedirectToAction(nameof(Index));
        }

        _db.CalendarEvents.Add(new CalendarEvent
        {
            Title = title.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            StartUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc),
            EndUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc),
            Visibility = CalendarVisibility.Private,
            OwnerUserId = user.Id
        });

        await _db.SaveChangesAsync();
        TempData["ToastOk"] = "Dodano prywatne wydarzenie.";
        return RedirectToAction(nameof(Index));
    }
}
