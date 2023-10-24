using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Answer> answers = await _context.Answers.ToListAsync();

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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /*[HttpGet]
        [Route("get-by-id")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                Answer a = await _context.Answers.FirstOrDefaultAsync(x => x.Id == id);
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
        }*/

        /*[HttpPost]
        public async Task<IActionResult> Create(CreateAnswer model)
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
                        DeletedAt = null,
                    };
                    _context.Answers.Add(data);
                    await _context.SaveChangesAsync();
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
        }*/

        [HttpPut]
        public async Task<IActionResult> Update(EditAnswer model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Answer existingAnswer = await _context.Answers.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);
                    if (existingAnswer != null)
                    {
                        Answer answer = new Answer
                        {
                            Id = model.id,
                            Content = model.content,
                            Status = model.status,
                            QuestionId = model.question_id,
                            CreatedAt = existingAnswer.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (answer != null)
                        {
                            _context.Answers.Update(answer);
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
                Answer answer = await _context.Answers.FindAsync(id);
                if (answer == null)
                    return NotFound();
                _context.Answers.Remove(answer);
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
                List<Answer> answers = await _context.Answers.Where(p => p.QuestionId == questionId).ToListAsync();
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
                    return NotFound("No answer found in this question.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
