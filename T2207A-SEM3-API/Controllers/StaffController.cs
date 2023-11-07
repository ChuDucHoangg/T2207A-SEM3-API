using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2207A_SEM3_API.DTOs;
using T2207A_SEM3_API.Entities;
using T2207A_SEM3_API.Models.Staff;
using T2207A_SEM3_API.Models.Student;
using T2207A_SEM3_API.Service.UploadFiles;

namespace T2207A_SEM3_API.Controllers
{
    [Route("api/staff")]
    [ApiController]
    public class StaffController : Controller
    {
        private readonly ExamonimyContext _context;
        private readonly IImgService _imgService;


        public StaffController(ExamonimyContext context, IImgService imgService)
        {
            _context = context;
            _imgService = imgService;
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

        private async Task<string> GenerateStaffCode()
        {
            // Lấy năm hiện tại dưới dạng chuỗi (ví dụ: "2022")
            string currentYear = DateTime.Now.ToString("yy");

            // Lấy tháng hiện tại dưới dạng chuỗi (ví dụ: "04" cho tháng 4)
            string currentMonth = DateTime.Now.ToString("MM");

            var codePrefix = $"TV{currentYear}{currentMonth:D2}";

            var lastStudent = await _context.Staffs
            .Where(s => s.StaffCode.StartsWith(codePrefix))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

            int newSequenceNumber;
            if (lastStudent != null)
            {
                var lastSequenceNumber = int.Parse(lastStudent.StaffCode.Substring(8));
                newSequenceNumber = lastSequenceNumber + 1;
            }
            else
            {
                newSequenceNumber = 1;
            }

            string studentCode = $"{codePrefix}{newSequenceNumber:D3}";

            return studentCode;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateStaff model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool emailExists = await _context.Staffs.AnyAsync(c => c.Email == model.email);

                    if (emailExists)
                    {
                        return BadRequest("Staff code already exists");
                    }

                    string imageUrl = await _imgService.UploadImageAsync(model.avatar);

                    // hash password
                    var salt = BCrypt.Net.BCrypt.GenerateSalt(10);
                    var hassPassword = BCrypt.Net.BCrypt.HashPassword(model.password, salt);

                    if (imageUrl != null)
                    {
                        Staff data = new Staff
                        {
                            StaffCode = await GenerateStaffCode(),
                            Fullname = model.fullname,
                            Avatar = imageUrl,
                            Birthday = model.birthday,
                            Email = model.email,
                            Phone = model.phone,
                            Gender = model.gender,
                            Address = model.address,
                            Password = hassPassword,
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

                        // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu hay chưa (trừ trường hợp cập nhật cùng mã)
                        bool emailExists = await _context.Staffs.AnyAsync(c => c.Email == model.email && c.Id != model.id);

                        if (emailExists)
                        {
                            return BadRequest("Staff code already exists");
                        }

                        Staff staff = new Staff
                        {
                            Id = model.id,
                            StaffCode = exexistingStaff.StaffCode,
                            Fullname = model.fullname,
                            Birthday = model.birthday,
                            Email = model.email,
                            Phone = model.phone,
                            Gender = model.gender,
                            Address = model.address,
                            Password = exexistingStaff.StaffCode,
                            Role = exexistingStaff.StaffCode,
                            CreatedAt = exexistingStaff.CreatedAt,
                            UpdatedAt = DateTime.Now,
                            DeletedAt = null,
                        };

                        if (model.avatar != null)
                        {
                            string imageUrl = await _imgService.UploadImageAsync(model.avatar);

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
        public async Task<IActionResult> GetbyRole(string role)
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
