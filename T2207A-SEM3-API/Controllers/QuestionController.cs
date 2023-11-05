using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Course;
using T2207A_SEM3_API.Models.General;
using T2207A_SEM3_API.Models.Question;
using T2207A_SEM3_API.Service.ClassCourses;
using T2207A_SEM3_API.Service.Questions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/question")]
    [ApiController]
    public class QuestionController : Controller
    {
        private readonly ExamonimyContext _context;
        private readonly IQuestionService _questionService;

        public QuestionController(ExamonimyContext context, IQuestionService questionService)
        {
            _context = context;
            _questionService = questionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Question> questions = await _context.Questions.Include(q => q.Answers).ToListAsync();

                var questionAnswers = await _questionService.GetTestQuestionsAnswers(questions);

                return Ok(questionAnswers);
                
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
                    QuestionDTO question = await _questionService.CreateQuestionMultipleChoice(model);

                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 201, // Sử dụng 201 Created
                        Message = "Course created successfully",
                        Data = question
                    };

                    return Created($"get-by-id?id={question.id}", response);
                }
                catch (Exception ex)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    };

                    return BadRequest(response);
                }
            }
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
        }

        [HttpPost("essay")]
        public async Task<IActionResult> CreateEssay(CreateQuestionEssay model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    QuestionDTO question = await _questionService.CreateQuestionEssay(model);

                    var response = new GeneralServiceResponse
                    {
                        Success = true,
                        StatusCode = 201, // Sử dụng 201 Created
                        Message = "Question created successfully",
                        Data = question
                    };

                    return Created($"get-by-id?id={question.id}", response);
                }
                catch (Exception ex)
                {
                    var response = new GeneralServiceResponse
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = ex.Message,
                        Data = ""
                    };

                    return BadRequest(response);
                }
            }
            var validationErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage);

            var validationResponse = new GeneralServiceResponse
            {
                Success = false,
                StatusCode = 400,
                Message = "Validation errors",
                Data = string.Join(" | ", validationErrors)
            };

            return BadRequest(validationResponse);
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
                            CourseId = existingQuestion.CourseId,
                            QuestionType = existingQuestion.QuestionType,
                            CreatedAt = existingQuestion.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };
                        // type là trắc nghiệm
                        if(existingQuestion.QuestionType == 0)
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

    }
}
