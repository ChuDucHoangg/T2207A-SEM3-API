using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Staff
{
    public int Id { get; set; }

    public string StaffCode { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
