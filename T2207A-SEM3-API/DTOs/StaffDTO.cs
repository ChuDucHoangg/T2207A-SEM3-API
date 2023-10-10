using Microsoft.AspNetCore.Identity;

namespace T2207A_SEM3_API.DTOs
{
    public class StaffDTO
    {
        public int id { get; set; }

        public string staff_code { get; set; }
        public string avatar { get; set; }

        public string fullname { get; set; }


        public DateTime birthday { get; set; }

        public string gender { get; set; }

        public string address { get; set; }

        public string email { get; set; }

        public string phone { get; set; }

        public string password { get; set; }

        public int role { get; set; }


        public DateTime? createdAt { get; set; }

        public DateTime? updateAt { get; set; }

        public DateTime? deleteAt { get; set; }

       
    }
}
