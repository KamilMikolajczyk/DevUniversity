using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Data;
using University.Models;

namespace University.Controllers;

[Authorize(Roles = "Student")]
public class StudentsController : Controller
{
    private readonly UnivercityDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentsController(UnivercityDbContext db, UserManager<ApplicationUser> userManager)
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
            .ThenInclude(ss => ss.Subject)
            .FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

        return View(student);
    }
}
