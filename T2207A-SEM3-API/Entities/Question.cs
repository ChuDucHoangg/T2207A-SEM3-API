using System;
using System.Collections.Generic;

namespace T2207A_SEM3_API.Entities;

public partial class Question
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int TestId { get; set; }

    public int Level { get; set; }

    public int QuestionType { get; set; }

    public double Score { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<AnswersForStudent> AnswersForStudents { get; set; } = new List<AnswersForStudent>();

    public virtual Test Test { get; set; } = null!;
}
