using System.ComponentModel.DataAnnotations;

namespace University.Models;

public class StudentGroup
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = default!; // e.g. INF2-B_26/27

    public Faculty Faculty { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<GroupSubject> GroupSubjects { get; set; } = new List<GroupSubject>();
}
