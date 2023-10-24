using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class ClassCourse
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int CourseId { get; set; }

    public int Status { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;

    public virtual Staff CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
