namespace University.Models.ViewModels;

public class SubjectGroupsVm
{
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = default!;

    public List<GroupRow> Groups { get; set; } = new();

    public class GroupRow
    {
        public int GroupId { get; set; }
        public string Code { get; set; } = default!;
        public bool Assigned { get; set; }
    }
}
