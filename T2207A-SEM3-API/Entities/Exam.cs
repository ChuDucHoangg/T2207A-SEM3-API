using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Exam
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public int? TeacherId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ExamAgain> ExamAgains { get; set; } = new List<ExamAgain>();

    public virtual ICollection<RegisterExam> RegisterExams { get; set; } = new List<RegisterExam>();

    public virtual Teacher? Teacher { get; set; }

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
