using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.Grade;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/grade")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly ExamonimyContext _context;

        public GradeController(ExamonimyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Grade> grades = await _context.Grades.ToListAsync();

                List<GradeDTO> data = new List<GradeDTO>();
                foreach (Grade g in grades)
                {
                    data.Add(new GradeDTO
                    {
                        id = g.Id,
                        student_id = g.StudentId,
                        test_id = g.TestId,
                        score = g.Score,
                        status = g.Status,
                        time_taken = g.TimeTaken,
                        createdAt = g.CreatedAt,
                        updatedAt = g.UpdatedAt,
                        deletedAt = g.DeletedAt
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
                Grade a = await _context.Grades.FirstOrDefaultAsync(x => x.Id == id);
                if (a != null)
                {
                    return Ok(new GradeDTO
                    {
                        id = a.Id,
                        student_id = a.StudentId,
                        test_id = a.TestId,
                        score = a.Score,
                        status = a.Status,
                        time_taken = a.TimeTaken,
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
        public async Task<IActionResult> Create(CreateGrade model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Grade data = new Grade
                    {
                        StudentId = model.student_id,
                        TestId = model.test_id,
                        Score = model.score,
                        Status = model.status,
                        TimeTaken = model.time_taken,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        DeletedAt = null,
                    };
                    _context.Grades.Add(data);
                    await _context.SaveChangesAsync();
                    return Created($"get-by-id?id={data.Id}", new GradeDTO
                    {
                        id = data.Id,
                        student_id = data.StudentId,
                        test_id = data.TestId,
                        score = data.Score,
                        status = data.Status,
                        time_taken = data.TimeTaken,
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
        public async Task<IActionResult> Update(EditGrade model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Grade existingAnswer = await _context.Grades.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);
                    if (existingAnswer != null)
                    {
                        Grade answer = new Grade
                        {
                            Id = model.id,
                            StudentId = model.student_id,
                            TestId = model.test_id,
                            Score = model.score,
                            Status = model.status,
                            TimeTaken = model.time_taken,
                            CreatedAt = existingAnswer.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (answer != null)
                        {
                            _context.Grades.Update(answer);
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
                Grade grade = await _context.Grades.FindAsync(id);
                if (grade == null)
                    return NotFound();
                _context.Grades.Remove(grade);
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
