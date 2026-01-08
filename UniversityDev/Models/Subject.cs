using System.ComponentModel.DataAnnotations;

namespace University.Models;

public class Subject
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = default!;

    public Faculty Faculty { get; set; }

    public int? LecturerId { get; set; }
    public Lecturer? Lecturer { get; set; }

    public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
    public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();

    public ICollection<GroupSubject> GroupSubjects { get; set; } = new List<GroupSubject>();
}
