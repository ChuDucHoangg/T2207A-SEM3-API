using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class AnswersForStudent
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public int TestId { get; set; }

    public string Content { get; set; } = null!;

    public int StudentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual Test Test { get; set; } = null!;
}
