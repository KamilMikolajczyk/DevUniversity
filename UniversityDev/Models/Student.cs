using System.ComponentModel.DataAnnotations;

namespace University.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    public string IndexNumber { get; set; } = default!;

    public Faculty Faculty { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();

    public int? StudentGroupId { get; set; }
    public StudentGroup? StudentGroup { get; set; }
}
