using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Answer
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public int Status { get; set; }

    public int QuestionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Question Question { get; set; } = null!;
}
