using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        //[Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Question> questions = await _context.Questions.Include(q => q.Answers).OrderByDescending(s => s.Id).Where(q => q.DeletedAt == null).ToListAsync();

                var questionAnswers = await _questionService.GetTestQuestionsAnswers(questions);

                return Ok(questionAnswers);
                
            } 
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("list-delete")]
        //[Authorize]
        public async Task<IActionResult> IndexDelete()
        {
            try
            {
                List<Question> questions = await _context.Questions.Include(q => q.Answers).OrderByDescending(s => s.Id).Where(q => q.DeletedAt != null).ToListAsync();

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
        [Authorize]
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

        [HttpPut("multiple-choice")]
        [Authorize]
        public async Task<IActionResult> UpdateMultipleChoice(EditQuestionMultipleChoice model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool updated = await _questionService.UpdateQuestionMultipleChoice(model);

                    if (updated)
                    {
                        var response = new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 204, // Sử dụng 204 No Content
                            Message = "Question updated successfully",
                            Data = ""
                        };

                        return NoContent();
                    }
                    else
                    {
                        var notFoundResponse = new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Question not found",
                            Data = ""
                        };

                        return NotFound(notFoundResponse);
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return BadRequest();
        }

        [HttpPut("essay")]
        [Authorize]
        public async Task<IActionResult> UpdateEssay(EditQuestionEssay model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool updated = await _questionService.UpdateQuestionEssay(model);

                    if (updated)
                    {
                        var response = new GeneralServiceResponse
                        {
                            Success = true,
                            StatusCode = 204, // Sử dụng 204 No Content
                            Message = "Question updated successfully",
                            Data = ""
                        };

                        return NoContent();
                    }
                    else
                    {
                        var notFoundResponse = new GeneralServiceResponse
                        {
                            Success = false,
                            StatusCode = 404,
                            Message = "Question not found",
                            Data = ""
                        };

                        return NotFound(notFoundResponse);
                    }
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            return BadRequest();
        }

        [HttpPut("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Question question = await _context.Questions.FindAsync(id);
                if (question == null)
                    return NotFound();

                question.DeletedAt = DateTime.Now;

                _context.Questions.Update(question);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("restore/{id}")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                Question question = await _context.Questions.FindAsync(id);
                if (question == null)
                    return NotFound();

                question.DeletedAt = null;

                _context.Questions.Update(question);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("permanently-delete/{id}")]
        public async Task<IActionResult> PermanentlyDelete(int id)
        {
            try
            {
                Question question = await _context.Questions.FindAsync(id);
                if (question == null)
                    return NotFound();

                _context.Questions.Remove(question); // Xóa bản ghi sinh viên hoàn toàn khỏi cơ sở dữ liệu

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
