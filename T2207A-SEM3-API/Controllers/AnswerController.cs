using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/answer")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly ExamonimyContext _context;

        public AnswerController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Answer> answers = _context.Answers.ToList();

            List<AnswerDTO> data = new List<AnswerDTO>();
            foreach (Answer a in answers)
            {
                data.Add(new AnswerDTO
                {
                    id = a.Id,
                    content = a.Content,
                    status = a.Status,
                    question_id = a.QuestionId,
                    createdAt = a.CreatedAt,
                    updatedAt = a.UpdatedAt,
                    deletedAt = a.DeletedAt
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
                Answer a = _context.Answers.FirstOrDefault(x => x.Id == id);
                if (a != null)
                {
                    return Ok(new AnswerDTO
                    {
                        id = a.Id,
                        content = a.Content,
                        status = a.Status,
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
        public IActionResult Create(CreateAnswer model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Answer data = new Answer
                    {
                        Content = model.content,
                        Status = model.status,
                        QuestionId = model.question_id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = DateTime.Now,
                    };
                    _context.Answers.Add(data);
                    _context.SaveChanges();
                    return Created($"get-by-id?id={data.Id}", new AnswerDTO
                    {
                        id = data.Id,
                        content = data.Content,
                        status = data.Status,
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
        public IActionResult Update(EditAnswer model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Answer answer = new Answer
                    {
                        Id = model.id,
                        Content = model.content,
                        Status = model.status,
                        QuestionId = model.question_id,
                        CreatedAt = model.createdAt,
                        UpdatedAt = model.updatedAt,
                        DeletedAt = model.deletedAt,
                    };

                    if (answer != null)
                    {
                        _context.Answers.Update(answer);
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
                Answer answer = _context.Answers.Find(id);
                if (answer == null)
                    return NotFound();
                _context.Answers.Remove(answer);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-questionId")]
        public IActionResult GetbyCategory(int questionId)
        {
            try
            {
                List<Answer> answers = _context.Answers.Where(p => p.QuestionId == questionId).ToList();
                if (answers != null)
                {
                    List<AnswerDTO> data = answers.Select(q => new AnswerDTO
                    {
                        id = q.Id,
                        content = q.Content,
                        status = q.Status,
                        question_id = q.QuestionId,
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
