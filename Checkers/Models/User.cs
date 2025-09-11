using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Models
{
    public class User
    {
        public string? FullName { get; set; }

        public string? UserName { get; set; }
        public string? Email { get; set; }

        public string? MobileNo { get; set; }

        public string? Password { get; set; }

        public DateTime BirthDate { get; set; }
        public string Photo { get; set; }

    }
}
