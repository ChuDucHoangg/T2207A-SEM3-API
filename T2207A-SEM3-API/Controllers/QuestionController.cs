using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
        {
            List<Question> questions = _context.Questions.ToList();

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

        [HttpGet]
        [Route("get-by-id")]
        public IActionResult Get(int id)
        {
            try
            {
                Question q = _context.Questions.FirstOrDefault(x => x.Id == id);
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
        public IActionResult Create(CreateQuestion model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Question data = new Question
                    {
                        Title = model.title,
                        TestId = model.test_id,
                        Level = model.level,
                        Score = model.score,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Questions.Add(data);
                    _context.SaveChanges();
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
        public IActionResult Update(EditQuestion model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Question question = new Question
                    {
                        Id = model.id,
                        Title = model.title,
                        TestId = model.test_id,
                        Level = model.level,
                        Score = model.score,
                        CreatedAt = model.createdAt,
                        UpdatedAt = model.updatedAt,
                        DeletedAt = model.deletedAt,
                    };

                    if (question != null)
                    {
                        _context.Questions.Update(question);
                        _context.SaveChanges();
                        return NoContent();
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
        public IActionResult Delete(int id)
        {
            try
            {
                Question question = _context.Questions.Find(id);
                if (question == null)
                    return NotFound();
                _context.Questions.Remove(question);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-testId")]
        public IActionResult GetbyCategory(int testId)
        {
            try
            {
                List<Question> questions = _context.Questions.Where(p => p.TestId == testId).ToList();
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
