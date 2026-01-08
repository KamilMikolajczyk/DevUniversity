namespace University.Models;

public class GroupSubject
{
    public int StudentGroupId { get; set; }
    public StudentGroup StudentGroup { get; set; } = default!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = default!;
}
