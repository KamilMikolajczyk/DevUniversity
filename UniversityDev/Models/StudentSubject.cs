namespace University.Models;

public class StudentSubject
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = default!;
}
