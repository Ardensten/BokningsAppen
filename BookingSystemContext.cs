using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BokningsAppen.Models;

namespace BokningsAppen
{
    internal class BookingSystemContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Day> Days { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=tcp:jonatansserver.database.windows.net,1433;Initial Catalog=JonatansDatabas;Persist Security Info=False;User ID=JontesAdmin;Password=RPVA2eS!X^A3r59n;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }

    }
}
