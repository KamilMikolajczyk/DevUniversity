using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using University.Models;

namespace University.Data;

public static class SeedData
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Student = "Student";
        public const string Lecturer = "Lecturer";
    }
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<UnivercityDbContext>();

        foreach (var role in new[] { Roles.Admin, Roles.Student, Roles.Lecturer })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
        var adminEmail = "admin@uni.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        var lecturerEmail ="lecturer@uni.local";
        var lecturerUser = await userManager.FindByEmailAsync(lecturerEmail);
        if (lecturerUser == null)
        {
            lecturerUser = new ApplicationUser { UserName = lecturerEmail, Email = lecturerEmail, EmailConfirmed = true };
            await userManager.CreateAsync(lecturerUser, "Lecturer123!");
            await userManager.AddToRoleAsync(lecturerUser, Roles.Lecturer);
        }

        // Ensure Lecturer profile exists
        if (!await db.Lecturers.AnyAsync(l => l.ApplicationUserId == lecturerUser.Id))
        {
            db.Lecturers.Add(new Lecturer
            {
                ApplicationUserId = lecturerUser.Id,
                Title = "Dr",
                Faculty = Faculty.ComputerScience
            });
            await db.SaveChangesAsync();
        }
        if (!await db.Subjects.AnyAsync())
        {
            var subject = new Subject { Name = "Databases", Faculty = Faculty.ComputerScience };
            db.Subjects.Add(subject);
            await db.SaveChangesAsync();

            db.CalendarEvents.Add(new CalendarEvent
            {
                Title = "Databases — Lecture 1",
                Location = "Room 101",
                StartUtc = DateTime.UtcNow.AddDays(1).Date.AddHours(8),
                EndUtc = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
                Visibility = CalendarVisibility.PublicToFaculty,
                Faculty = Faculty.ComputerScience,
                SubjectId = subject.Id
            });

            await db.SaveChangesAsync();
        }
    }
}
