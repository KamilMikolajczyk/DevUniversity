using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using University.Data;
using University.Models;

namespace University.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UnivercityDbContext _db;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, UnivercityDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string AccountType { get; set; } = "Student"; // Student / Lecturer

        [Required]
        public Faculty Faculty { get; set; } = Faculty.ComputerScience;

        public string? IndexNumber { get; set; }
        public string? LecturerTitle { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (Input.AccountType == "Student" && string.IsNullOrWhiteSpace(Input.IndexNumber))
        {
            ModelState.AddModelError("Input.IndexNumber", "Numer indeksu jest wymagany.");
            return Page();
        }

        if (Input.AccountType == "Lecturer" && string.IsNullOrWhiteSpace(Input.LecturerTitle))
        {
            ModelState.AddModelError("Input.LecturerTitle", "Tytuł jest wymagany.");
            return Page();
        }

        var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, EmailConfirmed = true };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        if (Input.AccountType == "Student")
        {
            await _userManager.AddToRoleAsync(user, "Student");
            _db.Students.Add(new Student { ApplicationUserId = user.Id, Faculty = Input.Faculty, IndexNumber = Input.IndexNumber!.Trim() });
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "Lecturer");
            _db.Lecturers.Add(new Lecturer { ApplicationUserId = user.Id, Faculty = Input.Faculty, Title = Input.LecturerTitle!.Trim() });
        }

        await _db.SaveChangesAsync();
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Input.AccountType == "Student" ? LocalRedirect("/Students") : LocalRedirect("/Calendar");
    }
}
