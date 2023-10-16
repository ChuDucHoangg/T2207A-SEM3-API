using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Student
{
    public int Id { get; set; }

    public string StudentCode { get; set; } = null!;

    public string Fullname { get; set; } = null!;

    public string Avatar { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Address { get; set; } = null!;

    public int ClassId { get; set; }

    public string Password { get; set; } = null!;

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AnswersForStudent> AnswersForStudents { get; set; } = new List<AnswersForStudent>();

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<ExamAgain> ExamAgains { get; set; } = new List<ExamAgain>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<RegisterExam> RegisterExams { get; set; } = new List<RegisterExam>();

    public virtual ICollection<StudentTest> StudentTests { get; set; } = new List<StudentTest>();
}
