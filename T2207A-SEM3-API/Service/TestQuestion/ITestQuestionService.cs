using Microsoft.AspNetCore.Mvc;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Question;
using T2207A_SEM3_API.Models.Test;

namespace T2207A_SEM3_API.Service.ClassCourses
{
    public interface ITestQuestionService
    {
        Task<List<QuestionAnswerResponse>> GetTestQuestionsForTestDetail(int testId);
        Task<Test> TestExists(int testId);
        Task<StudentTest> IsTestNotTaken(int testId, int studentId);
    }
}
