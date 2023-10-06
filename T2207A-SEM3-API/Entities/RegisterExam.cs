using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class RegisterExam
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int ExamId { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
