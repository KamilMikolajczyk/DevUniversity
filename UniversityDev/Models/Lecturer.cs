using System.ComponentModel.DataAnnotations;

namespace University.Models;

public class Lecturer
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = "Dr";

    public Faculty Faculty { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
