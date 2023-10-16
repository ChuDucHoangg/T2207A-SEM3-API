using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.AnswerForStudent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/answersForStudent")]
    [ApiController]
    public class AnswersForStudentController : Controller
    {
        private readonly ExamonimyContext _context;

        public AnswersForStudentController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpPost("submit-exam")]
        public async Task<IActionResult> SubmitAnswer(List<CreateAnswerForStudent> answersForStudents, int test_id)
        {
            try
            {
                // danh sách câu hỏi
                List<Question> questions = await _context.Questions.Where(p => p.TestId == test_id).ToListAsync();

                // danh sách câu trả lời đúng
                List<Answer> answerCorrect = await _context.Answers.Where(p => p.Question.TestId == test_id && p.Status == 1).ToListAsync();

                // lưu câu trả lời của student
                List<AnswersForStudent> answersForStudents1 = new List<AnswersForStudent>();

                foreach (var answer in answersForStudents)
                {
                    AnswersForStudent answersForStudent = new AnswersForStudent
                    {
                        StudentId = answer.student_id,
                        Content = answer.content,
                        QuestionId = answer.question_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };

                    _context.AnswersForStudents.Add(answersForStudent);
                    await _context.SaveChangesAsync();

                    answersForStudents1.Add(answersForStudent);

                }

                // tính điểm
                var score = CalculateScore(questions, answerCorrect, answersForStudents1);

                var test = await _context.Tests.FindAsync(test_id);

                // tạo điểm
                var grade = new Grade
                {
                    StudentId = answersForStudents1[0].StudentId,
                    Score = score,
                    TestId = test_id,
                    TimeTaken = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = null
                };

                // kiểm tra đỗ hay chưa
                if (score > test.PastMarks)
                {
                    grade.Status = 1;
                }
                else
                {
                    grade.Status = 0;
                }

                _context.Add(grade);
                await _context.SaveChangesAsync();

                return Ok(grade);



            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        private double CalculateScore(List<Question> questions, List<Answer> correctAnswers, List<AnswersForStudent> studentAnswers)
        {
            double totalScore = 0;

            foreach (var question in questions)
            {
                // Tìm câu trả lời đúng cho câu hỏi hiện tại
                var correctAnswer = correctAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                if (correctAnswer != null)
                {
                    // Tìm câu trả lời của sinh viên cho câu hỏi hiện tại
                    var studentAnswer = studentAnswers.FirstOrDefault(sa => sa.QuestionId == question.Id);

                    if (studentAnswer != null && studentAnswer.Content == correctAnswer.Content)
                    {
                        // Nếu câu trả lời của sinh viên giống với đáp án đúng, cộng thêm điểm
                        totalScore += question.Score;
                    }
                }
            }

            return totalScore;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<AnswersForStudent> answersForStudents = await _context.AnswersForStudents.ToListAsync();

                List<AnswerForStudentDTO> data = new List<AnswerForStudentDTO>();
                foreach (AnswersForStudent a in answersForStudents)
                {
                    data.Add(new AnswerForStudentDTO
                    {
                        id = a.Id,
                        student_id = a.StudentId,
                        content = a.Content,
                        question_id = a.QuestionId,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt
                    });
                }
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                AnswersForStudent a = await _context.AnswersForStudents.FirstOrDefaultAsync(x => x.Id == id);
                if (a != null)
                {
                    return Ok(new AnswerForStudentDTO
                    {
                        id = a.Id,
                        student_id = a.StudentId,
                        content = a.Content,
                        question_id = a.QuestionId,
                        createdAt = a.CreatedAt,
                        updatedAt = a.UpdatedAt,
                        deletedAt = a.DeletedAt

                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAnswerForStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AnswersForStudent data = new AnswersForStudent
                    {
                        StudentId = model.student_id,
                        Content = model.content,
                        QuestionId = model.question_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.AnswersForStudents.Add(data);
                    await _context.SaveChangesAsync();
                    return Created($"get-by-id?id={data.Id}", new AnswerForStudentDTO
                    {
                        id = data.Id,
                        student_id = data.StudentId,
                        content = data.Content,
                        question_id = data.QuestionId,
                        createdAt = data.CreatedAt,
                        updatedAt = data.UpdatedAt,
                        deletedAt = data.DeletedAt
                    });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            var msgs = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);
            return BadRequest(string.Join(" | ", msgs));
        }

        [HttpPut]
        public async Task<IActionResult> Update(EditAnswerForStudent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    AnswersForStudent existingAnswer = await _context.AnswersForStudents.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);
                    if (existingAnswer != null)
                    {
                        AnswersForStudent answer = new AnswersForStudent
                        {
                            Id = model.id,
                            StudentId = model.student_id,
                            Content = model.content,
                            QuestionId = model.question_id,
                            CreatedAt = existingAnswer.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (answer != null)
                        {
                            _context.AnswersForStudents.Update(answer);
                            await _context.SaveChangesAsync();
                            return NoContent();
                        }
                    }
                    else
                    {
                        return NotFound(); // Không tìm thấy lớp để cập nhật
                    }



                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return BadRequest();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                AnswersForStudent answer = await _context.AnswersForStudents.FindAsync(id);
                if (answer == null)
                    return NotFound();
                _context.AnswersForStudents.Remove(answer);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-questionId")]
        public async Task<IActionResult> GetbyQuestion(int questionId)
        {
            try
            {
                List<AnswersForStudent> answers = await _context.AnswersForStudents.Where(p => p.QuestionId == questionId).ToListAsync();
                if (answers != null)
                {
                    List<AnswerForStudentDTO> data = answers.Select(q => new AnswerForStudentDTO
                    {
                        id = q.Id,
                        student_id = q.StudentId,
                        content = q.Content,
                        question_id = q.QuestionId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No answer found in this question.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-testIdAndStudentId")]
        public async Task<IActionResult> GetbyTestAndStudent(int testID, int studentID)
        {
            try
            {
                List<AnswersForStudent> answers = await _context.AnswersForStudents.Where(p => p.StudentId == studentID && p.Question.TestId == testID).ToListAsync();
                if (answers != null)
                {
                    List<AnswerForStudentDTO> data = answers.Select(q => new AnswerForStudentDTO
                    {
                        id = q.Id,
                        student_id = q.StudentId,
                        content = q.Content,
                        question_id = q.QuestionId,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No answer found in this Test.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


    }
}
