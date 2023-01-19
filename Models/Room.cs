using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BokningsAppen.Models
{
    internal class Room
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        public int Seats { get; set; }
        public int ElectricalOutlets { get; set; }
        public bool Window { get; set; }
        public bool Projector { get; set; }
        public bool Whiteboard { get; set; }
    }
}
