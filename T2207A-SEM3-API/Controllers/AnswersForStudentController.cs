﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Answer;
using T2207A_SEM3_API.Models.AnswerForStudent;

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

        [HttpGet]
        public  async Task<IActionResult> Index()
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
                        student_id=a.StudentId,
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
