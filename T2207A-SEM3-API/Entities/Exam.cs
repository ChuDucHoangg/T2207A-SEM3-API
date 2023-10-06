using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Exam
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int CourseId { get; set; }

    public DateTime StartDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<RegisterExam> RegisterExams { get; set; } = new List<RegisterExam>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
