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
        public IActionResult Index()
        {
            List<Staff> staffs = _context.Staffs.ToList();

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
                    Role = sa.Role,
                    createdAt = sa.CreatedAt,
                    updateAt = sa.UpdatedAt,
                    deleteAt = sa.DeletedAt
                });
            }
            return Ok(data);
        }

        [HttpGet]
        [Route("get-by-codeStaff")]
        public IActionResult Get(int id)
        {
            try
            {
                Staff sa = _context.Staffs.Find(id);
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
                        Role = sa.Role,
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




        [HttpPost]
        public IActionResult Create(CreateStaff model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool codeExists = _context.Staffs.Any(c => c.StaffCode == model.staff_code);

                    if (codeExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Code student already exists");
                    }

                    Staff data = new Staff
                    {
                        StaffCode = model.staff_code,
                        Fullname = model.fullname,
                        Avatar = model.avatar,
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
                    _context.SaveChanges();
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
                        Role = data.Role,
                        createdAt = data.CreatedAt,
                        updateAt = data.UpdatedAt,
                        deleteAt = data.DeletedAt,
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
        public IActionResult Update(EditStaff model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra xem name đã tồn tại trong cơ sở dữ liệu hay chưa
                    bool codeExists = _context.Staffs.Any(c => c.StaffCode == model.staff_code);

                    if (codeExists)
                    {
                        // Nếu name đã tồn tại, trả về BadRequest hoặc thông báo lỗi tương tự
                        return BadRequest("Code student already exists");
                    }

                    Staff exexistingStaff = _context.Staffs.AsNoTracking().FirstOrDefault(e => e.Id == model.id);

                    if (exexistingStaff != null)
                    {
                        Staff staff = new Staff
                        {
                            Id = model.id,
                            StaffCode = model.staff_code,
                            Fullname = model.fullname,
                            Avatar = model.avatar,
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

                        if (staff != null)
                        {
                            _context.Staffs.Update(staff);
                            _context.SaveChanges();
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
        public IActionResult Delete(int id)
        {
            try
            {
                Staff staff = _context.Staffs.Find(id);
                if (staff == null)
                    return NotFound();
                _context.Staffs.Remove(staff);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
