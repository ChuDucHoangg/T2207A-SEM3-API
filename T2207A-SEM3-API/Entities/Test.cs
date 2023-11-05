using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Test
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int ExamId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public double PastMarks { get; set; }

    public double TotalMarks { get; set; }

    public int TypeTest { get; set; }

    public int NumberOfQuestionsInExam { get; set; }

    public int CreatedBy { get; set; }

    public int Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Staff CreatedByNavigation { get; set; } = null!;

    public virtual Exam Exam { get; set; } = null!;

    public virtual ICollection<ExamAgain> ExamAgains { get; set; } = new List<ExamAgain>();

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual ICollection<QuestionTest> QuestionTests { get; set; } = new List<QuestionTest>();

    public virtual ICollection<StudentTest> StudentTests { get; set; } = new List<StudentTest>();
}
