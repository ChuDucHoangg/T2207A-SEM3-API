using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Staff;
using T2207A_SEM3_API.Models.Student;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/staff")]
    [ApiController]
    public class StaffController : Controller
    {
        private readonly ExamonimyContext _context;

        public StaffController(ExamonimyContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Staff> staffs = await _context.Staffs.ToListAsync();

                List<StaffDTO> data = new List<StaffDTO>();

                foreach (Staff sa in staffs)
                {
                    data.Add(new StaffDTO
                    {
                        id = sa.Id,
                        staff_code = sa.StaffCode,
                        fullname = sa.Fullname,
                        avatar = sa.Avatar,
                        birthday = sa.Birthday,
                        email = sa.Email,
                        phone = sa.Phone,
                        gender = sa.Gender,
                        address = sa.Address,
                        password = sa.Password,
                        role = sa.Role,
                        createdAt = sa.CreatedAt,
                        updateAt = sa.UpdatedAt,
                        deleteAt = sa.DeletedAt
                    });
                }
                return Ok(data);
            } 
            catch (Exception e)
            {
                return BadRequest($"An error occurred: {e.Message}");
            }
        }

        [HttpGet]
        [Route("get-by-codeStaff")]
        public async Task<IActionResult> Get(string code_staff)
        {
            try
            {
                Staff sa = await _context.Staffs.AsNoTracking().FirstOrDefaultAsync(e => e.StaffCode == code_staff);

                if (sa != null)
                {
                    return Ok(new StaffDTO
                    {
                        id = sa.Id,
                        staff_code = sa.StaffCode,
                        fullname = sa.Fullname,
                        avatar = sa.Avatar,
                        birthday = sa.Birthday,
                        email = sa.Email,
                        phone = sa.Phone,
                        gender = sa.Gender,
                        address = sa.Address,
                        password = sa.Password,
                        role = sa.Role,
                        createdAt = sa.CreatedAt,
                        updateAt = sa.UpdatedAt,
                        deleteAt = sa.DeletedAt

                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NotFound();
        }

        private async Task<string> UploadImageAsync(IFormFile avatar)
        {
            if (avatar != null && avatar.Length > 0)
            {
                // Lấy tên tệp tin
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";

                // Đường dẫn lưu trữ tệp tin
                string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                string filePath = Path.Combine(uploadDirectory, fileName);

                Directory.CreateDirectory(uploadDirectory);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                return $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
            }

            return null; // Trả về null nếu không có hình ảnh
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateStaff model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool codeExists = await _context.Staffs.AnyAsync(c => c.StaffCode == model.staff_code);

                    if (codeExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Code Staff already exists");
                    }

                    // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool emailExists = await _context.Staffs.AnyAsync(c => c.Email == model.email);

                    if (emailExists)
                    {
                        return BadRequest("Staff code already exists");
                    }

                    string imageUrl = await UploadImageAsync(model.avatar);

                    if (imageUrl != null)
                    {
                        Staff data = new Staff
                        {
                            StaffCode = model.staff_code,
                            Fullname = model.fullname,
                            Avatar = imageUrl,
                            Birthday = model.birthday,
                            Email = model.email,
                            Phone = model.phone,
                            Gender = model.gender,
                            Address = model.address,
                            Password = model.password,
                            Role = model.role,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };
                        _context.Staffs.Add(data);
                        await _context.SaveChangesAsync();
                        return Created($"get-by-id?id={data.Id}", new StaffDTO
                        {
                            id = data.Id,
                            staff_code = data.StaffCode,
                            fullname = data.Fullname,
                            avatar = data.Avatar,
                            birthday = data.Birthday,
                            email = data.Email,
                            phone = data.Phone,
                            gender = data.Gender,
                            address = data.Address,
                            password = data.Password,
                            role = data.Role,
                            createdAt = data.CreatedAt,
                            updateAt = data.UpdatedAt,
                            deleteAt = data.DeletedAt,
                        });
                    }
                    else
                    {
                        return BadRequest("Please provide an avatar.");
                    }
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
        public async Task<IActionResult> Update([FromForm] EditStaff model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Staff exexistingStaff = await _context.Staffs.AsNoTracking().FirstOrDefaultAsync(e => e.Id == model.id);

                    if (exexistingStaff != null)
                    {
                        // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng mã)
                        bool codeExists = await _context.Staffs.AnyAsync(c => c.StaffCode == model.staff_code && c.Id != model.id);

                        if (codeExists)
                        {
                            // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                            return BadRequest("Code Staff already exists");
                        }

                        // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng mã)
                        bool emailExists = await _context.Staffs.AnyAsync(c => c.Email == model.email && c.Id != model.id);

                        if (emailExists)
                        {
                            return BadRequest("Staff code already exists");
                        }

                        Staff staff = new Staff
                        {
                            Id = model.id,
                            StaffCode = model.staff_code,
                            Fullname = model.fullname,
                            Birthday = model.birthday,
                            Email = model.email,
                            Phone = model.phone,
                            Gender = model.gender,
                            Address = model.address,
                            Password = model.password,
                            Role = model.role,
                            CreatedAt = exexistingStaff.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (model.avatar != null)
                        {
                            string imageUrl = await UploadImageAsync(model.avatar);

                            if (imageUrl == null)
                            {
                                return BadRequest("Failed to upload avatar.");
                            }

                            staff.Avatar = imageUrl;
                        }
                        else
                        {
                            staff.Avatar = exexistingStaff.Avatar;
                        }

                        if (staff != null)
                        {
                            _context.Staffs.Update(staff);
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
                Staff staff = await _context.Staffs.FindAsync(id);
                if (staff == null)
                    return NotFound();
                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("get-by-role")]
        public async Task<IActionResult> GetbyRole(int role)
        {
            try
            {
                List<Staff> staffs = await _context.Staffs.Where(p => p.Role == role).ToListAsync();
                if (staffs != null)
                {
                    List<StaffDTO> data = staffs.Select(c => new StaffDTO
                    {
                        id = c.Id,
                        staff_code = c.StaffCode,
                        fullname = c.Fullname,
                        avatar = c.Avatar,
                        birthday = c.Birthday,
                        email = c.Email,
                        phone = c.Phone,
                        gender = c.Gender,
                        address = c.Address,
                        password = c.Password,
                        role = c.Role,
                        createdAt = c.CreatedAt,
                        updateAt = c.UpdatedAt,
                        deleteAt = c.DeletedAt
                    }).ToList();

                    return Ok(data);
                }
                else
                {
                    return NotFound("No Staff found in this role.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}
