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
                        finishedAt = g.FinishedAt,
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
                        finishedAt = a.FinishedAt,
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
                            FinishedAt = model.finished_at,
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
    }
}
