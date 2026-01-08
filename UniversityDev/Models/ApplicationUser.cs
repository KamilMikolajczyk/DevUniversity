using Microsoft.AspNetCore.Identity;

namespace University.Models;

public class ApplicationUser : IdentityUser
{
    public bool IsDeleted { get; set; }
    public Student? Student { get; set; }
    public Lecturer? Lecturer { get; set; }
}
