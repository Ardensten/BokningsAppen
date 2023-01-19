using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BokningsAppen.Models
{
    internal class User
    {
        public int Id { get; set; }
        public long SocialSecurityNumber { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool Admin { get; set; }
    }
}
