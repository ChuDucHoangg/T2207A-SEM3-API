using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Course
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int ClassId { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Staff CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}
