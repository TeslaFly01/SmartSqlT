using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Models
{
    public class PasswordResultDTO
    {
        public string Password { get; set; }

        public int PasswordStrength { get; set; }
    }
}
