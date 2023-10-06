using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Test
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int ExamId { get; set; }

    public int StudentId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int TotalQuestion { get; set; }

    public int PastMarks { get; set; }

    public int TotalMarks { get; set; }

    public int TeacherId { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Student Student { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
