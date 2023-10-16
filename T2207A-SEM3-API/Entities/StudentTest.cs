using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class StudentTest
{
    public int Id { get; set; }

    public int TestId { get; set; }

    public int StudentId { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Test Test { get; set; } = null!;
}
