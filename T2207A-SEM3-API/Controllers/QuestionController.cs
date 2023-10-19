using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Question;

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
                        test_id = q.TestId,
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
                        test_id = q.TestId,
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

        [HttpPost]
        public async Task<IActionResult> Create(CreateQuestion model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Question data = new Question
                    {
                        Title = model.title,
                        Level = model.level,
                        Score = model.score,
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
                        test_id = data.TestId,
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
                            TestId = model.test_id,
                            Level = model.level,
                            Score = model.score,
                            CreatedAt = existingQuestion.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

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
                List<Question> questions = await _context.Questions.Where(p => p.TestId == testId).ToListAsync();
                if (questions != null)
                {
                    List<QuestionDTO> data = questions.Select(q => new QuestionDTO
                    {
                        id = q.Id,
                        title = q.Title,
                        test_id = q.TestId,
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
