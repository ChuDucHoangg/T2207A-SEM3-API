using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Question;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/question")]
    [ApiController]
    public class QuestionController : Controller
    {
        private readonly ExamonimyContext _context;

        public QuestionController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Question> questions = await _context.Questions.ToListAsync();

                List<QuestionDTO> data = new List<QuestionDTO>();
                foreach (Question q in questions)
                {
                    data.Add(new QuestionDTO
                    {
                        id = q.Id,
                        title = q.Title,
                        level = q.Level,
                        score = q.Score,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
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
                Question q = await _context.Questions.FirstOrDefaultAsync(x => x.Id == id);
                if (q != null)
                {
                    return Ok(new QuestionDTO
                    {
                        id = q.Id,
                        title = q.Title,
                        level = q.Level,
                        score = q.Score,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt

                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        [HttpPost("multiple-choice")]
        public async Task<IActionResult> CreateMultipleChoice(CreateQuestionMultipleChoice model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Question data = new Question
                    {
                        Title = model.title,
                        Level = model.level,
                        QuestionType = 0,
                        CourseId = model.course_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    
                     // Thiết lập điểm (score) dựa trên mức độ (level)
                     if (model.level == 1)
                     {
                         data.Score = 3.85; // Điểm cho câu dễ
                     }
                     else if(model.level == 2)
                     {
                         data.Score = 6.41; // Điểm cho câu trung bình
                     }
                     else if (model.level == 3)
                     {
                         data.Score = 8.97; // Điểm cho câu khó
                     }
                     else
                     {
                        // Xử lý khi mức độ không xác định, có thể đặt điểm mặc định hoặc thông báo lỗi.
                         data.Score = 0.0; // Điểm mặc định hoặc giá trị khác tùy bạn
                     }

                    _context.Questions.Add(data);
                    await _context.SaveChangesAsync();

                    foreach (var answerModel in model.answers)
                    {
                        var answer = new Answer
                        {
                            Content = answerModel.content,
                            Status = answerModel.status,
                            QuestionId = data.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            DeletedAt = null,
                        };

                        _context.Answers.Add(answer);
                        await _context.SaveChangesAsync();
                    }


                    return Created($"get-by-id?id={data.Id}", new QuestionDTO
                    {
                        id = data.Id,
                        title = data.Title,
                        level = data.Level,
                        score = data.Score,
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

        [HttpPost("essay")]
        public async Task<IActionResult> CreateEssay(CreateQuestionEssay model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Question data = new Question
                    {
                        Title = model.title,
                        Level = 3,
                        QuestionType = 1,
                        CourseId = model.course_id,
                        Score = 100,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };


                    _context.Questions.Add(data);
                    await _context.SaveChangesAsync();


                    return Created($"get-by-id?id={data.Id}", new QuestionDTO
                    {
                        id = data.Id,
                        title = data.Title,
                        level = data.Level,
                        score = data.Score,
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
        public async Task<IActionResult> Update(EditQuestion model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Question existingQuestion = await _context.Questions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);

                    if (existingQuestion != null)
                    {
                        Question question = new Question
                        {
                            Id = model.id,
                            Title = model.title,
                            Level = model.level,
                            CourseId = model.course_id,
                            QuestionType = model.question_type,
                            CreatedAt = existingQuestion.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };
                        // type là trắc nghiệm
                        if(model.question_type == 0)
                        {
                            // Thiết lập điểm (score) dựa trên mức độ (level)
                            if (model.level == 1)
                            {
                                question.Score = 3.85; // Điểm cho câu dễ
                            }
                            else if (model.level == 2)
                            {
                                question.Score = 6.41; // Điểm cho câu trung bình
                            }
                            else if (model.level == 3)
                            {
                                question.Score = 8.97; // Điểm cho câu khó
                            }
                            else
                            {
                                // Xử lý khi mức độ không xác định, có thể đặt điểm mặc định hoặc thông báo lỗi.
                                question.Score = 0.0; // Điểm mặc định hoặc giá trị khác tùy bạn
                            }
                        }
                        question.Score = 100;
                        

                        if (question != null)
                        {
                            _context.Questions.Update(question);
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
                Question question = await _context.Questions.FindAsync(id);
                if (question == null)
                    return NotFound();
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-testId")]
        public async Task<IActionResult> GetbyCategory(int testId)
        {
            try
            {
                // Lấy danh sách ID của các câu hỏi thuộc bài thi
                var questionIds = await _context.QuestionTests
                    .Where(qt => qt.TestId == testId)
                    .OrderBy(qt => qt.Orders)
                    .Select(qt => qt.QuestionId)
                    .ToListAsync();

                // Lấy danh sách câu hỏi dựa trên các ID câu hỏi
               

                List<Question> questions = await _context.Questions
                    .Where(q => questionIds.Contains(q.Id))
                    .ToListAsync();
                if (questions != null)
                {
                    List<QuestionDTO> data = questions.Select(q => new QuestionDTO
                    {
                        id = q.Id,
                        title = q.Title,
                        level = q.Level,
                        score = q.Score,
                        createdAt = q.CreatedAt,
                        updatedAt = q.UpdatedAt,
                        deletedAt = q.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No products found in this category.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
