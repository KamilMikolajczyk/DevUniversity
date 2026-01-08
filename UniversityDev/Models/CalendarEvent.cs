using System.ComponentModel.DataAnnotations;

namespace University.Models;

public class CalendarEvent
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = default!;

    public string? Location { get; set; }

    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    public bool IsLecture { get; set; }

    public CalendarVisibility Visibility { get; set; }

    public int? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public string? OwnerUserId { get; set; }
    public ApplicationUser? OwnerUser { get; set; }

    public Faculty? Faculty { get; set; }
}
