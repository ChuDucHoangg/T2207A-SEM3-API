using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class QuestionTest
{
    public int Id { get; set; }

    public int TestId { get; set; }

    public int QuestionId { get; set; }

    public int Orders { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual Test Test { get; set; } = null!;
}
