using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University.Data;
using University.Models;
using University.Models.ViewModels;

namespace University.Controllers;

[Authorize(Roles = "Admin")]
public class AdminSubjectGroupsController : Controller
{
    private readonly UnivercityDbContext _db;

    public AdminSubjectGroupsController(UnivercityDbContext db)
    {
        _db = db;
    }

    // Step 1: choose subject
    public async Task<IActionResult> PickSubject()
    {
        var subjects = await _db.Subjects
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(subjects);
    }

    // Step 2: assign groups to selected subject (and enroll students)
    public async Task<IActionResult> Manage(int subjectId)
    {
        var subject = await _db.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
        if (subject == null) return NotFound();

        var assigned = await _db.GroupSubjects
            .Where(gs => gs.SubjectId == subjectId)
            .Select(gs => gs.StudentGroupId)
            .ToHashSetAsync();

        var groups = await _db.StudentGroups
            .OrderBy(g => g.Code)
            .ToListAsync();

        var vm = new SubjectGroupsVm
        {
            SubjectId = subject.Id,
            SubjectName = subject.Name,
            Groups = groups.Select(g => new SubjectGroupsVm.GroupRow
            {
                GroupId = g.Id,
                Code = g.Code,
                Assigned = assigned.Contains(g.Id)
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Manage(SubjectGroupsVm vm)
    {
        // current assignments
        var current = await _db.GroupSubjects
            .Where(gs => gs.SubjectId == vm.SubjectId)
            .Select(gs => gs.StudentGroupId)
            .ToHashSetAsync();

        // selected assignments
        var selected = vm.Groups
            .Where(g => g.Assigned)
            .Select(g => g.GroupId)
            .ToHashSet();

        var toAdd = selected.Except(current).ToList();
        var toRemove = current.Except(selected).ToList();

        foreach (var gid in toAdd)
            _db.GroupSubjects.Add(new GroupSubject { StudentGroupId = gid, SubjectId = vm.SubjectId });

        if (toRemove.Count > 0)
        {
            var rows = await _db.GroupSubjects
                .Where(gs => gs.SubjectId == vm.SubjectId && toRemove.Contains(gs.StudentGroupId))
                .ToListAsync();

            _db.GroupSubjects.RemoveRange(rows);
        }

        // enroll students for newly added groups
        foreach (var gid in toAdd)
        {
            var studentIds = await _db.Students
                .Where(s => s.StudentGroupId == gid)
                .Select(s => s.Id)
                .ToListAsync();

            var already = await _db.StudentSubjects
                .Where(ss => studentIds.Contains(ss.StudentId) && ss.SubjectId == vm.SubjectId)
                .Select(ss => ss.StudentId)
                .ToHashSetAsync();

            foreach (var sid in studentIds)
            {
                if (!already.Contains(sid))
                    _db.StudentSubjects.Add(new StudentSubject { StudentId = sid, SubjectId = vm.SubjectId });
            }
        }

        await _db.SaveChangesAsync();
        TempData["ToastOk"] = "Zapisano grupy dla przedmiotu.";
        return RedirectToAction(nameof(Manage), new { subjectId = vm.SubjectId });
    }
}
