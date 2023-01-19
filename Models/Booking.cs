using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BokningsAppen.Models
{
    internal class Booking
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public int DayId { get; set; }
        public int Week { get; set; }
    }
}
