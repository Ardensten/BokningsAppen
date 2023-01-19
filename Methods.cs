using BokningsAppen.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BokningsAppen
{

    internal class Methods
    {
        public static class Globals
        {
            public static int loggedInId = 0;
            public static int weekNumber = GetIso8601WeekOfYear(DateTime.Now);
        }
        static public void Run()
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;

            while (true)
            {
                StartPage();
            }
        }
        static public void StartPage()
        {
            Console.Clear();
            Globals.loggedInId = 0;
            Console.WriteLine($"Välkommen! Här kan du boka ett grupprum.\n\n" +

                "1. Logga in.\n" +
                "2. Registrera dig.");

            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.KeyChar)
            {
                case '1':
                    LogIn();
                    break;
                case '2':
                    SignUp();
                    break;
                default:
                    WrongInput();
                    break;
            }
            Console.ReadKey(true);
            Console.Clear();
        }
        private static void SignUp()
        {
            Console.Clear();

            Console.WriteLine($"Registrera dig här!\n");
            Console.Write("Ange personnummer (YYYYMMDDXXXX): ");
            long newSocialSecurityNumber = long.Parse(Console.ReadLine());
            Console.Write("Ange namn: ");
            string newName = Console.ReadLine();
            Console.WriteLine("Ange lösenord: ");
            string newPassword = Console.ReadLine();
            Console.Write("Adminrättigheter? (y/n): ");
            string newAdmin = Console.ReadLine().ToUpper();
            bool newAdminBool = false;

            if (newAdmin == "Y")
            {
                newAdminBool = true;
            }
            else if (newAdmin == "N")
            {
                newAdminBool = false;
            }
            else
            {
                WrongInput();
            }

            using (var db = new BookingSystemContext())
            {
                var newUser = new User
                {
                    SocialSecurityNumber = newSocialSecurityNumber,
                    Password = newPassword,
                    Name = newName,
                    Admin = newAdminBool,
                };
                var userList = db.Users;
                userList.Add(newUser);
                
                db.SaveChanges();
            }

            Console.WriteLine("Du är nu registerad! Tryck någonstans för att fortsätta.");
            Console.ReadKey();
            StartPage();
        }
        private static void LogIn()
        {
            Console.Clear();

            Console.WriteLine("Logga in här!\n");
            Console.Write("Ange personnummer: (YYYYMMDDXXXX): ");
            long socialSecurityNumber = long.Parse(Console.ReadLine());
            Console.Write("Ange lösenord: ");
            string password = Console.ReadLine();
            Console.WriteLine("Loggar in...");

            using (var db = new BookingSystemContext())
            {
                foreach (var user in db.Users.Where(x => x.SocialSecurityNumber == socialSecurityNumber && x.Password == password))
                {
                    Globals.loggedInId = user.Id;

                    if (user.Admin == true)
                    {
                        AdminStartPage();
                        break;
                    }
                    else if (user.Admin == false)
                    {
                        UserStartPage();
                        break;
                    }
                    break;
                }

                if (Globals.loggedInId == 0)
                {
                    Console.WriteLine("Något blev fel! Tryck någonstans för att fortsätta.");
                    Console.ReadKey();
                    LogIn();
                }
            }
        }

        //――――――――――――――――――――Användarens metoder―――――――――――――――――――――――――――――――――――――――――――――
        private static void UserStartPage()
        {
            Console.Clear();

            using (var db = new BookingSystemContext())
            {
                var userName = db.Users.Where(x => x.Id == Globals.loggedInId).Select(x => x.Name).FirstOrDefault();
                Console.WriteLine($"Välkommen {userName}!\n\n" +
                    "0. Logga ut.\n" +
                    "1. Boka nytt rum.\n" +
                    "2. Se dina bokningar.\n");

                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.KeyChar)
                {
                    case '0':
                        StartPage();
                        break;
                    case '1':
                        ViewAllRooms();
                        break;
                    case '2':
                        YourBookings();
                        break;
                    default:
                        WrongInput();
                        break;
                }
                Console.ReadKey(true);
                Console.Clear();
            }
        }
        public static void ViewAllRooms()
        {
            Console.Clear();

            using (var db = new BookingSystemContext())
            {
                var userName = db.Users.Where(x => x.Id == Globals.loggedInId).Select(x => x.Name).FirstOrDefault();

                var bookings = from b in db.Bookings
                               join r in db.Rooms on b.RoomId equals r.Id
                               join u in db.Users on b.UserId equals u.Id
                               where b.Week == Globals.weekNumber
                               select new { Day = b.DayId, RoomName = r.RoomName, UserName = u.Name, RoomId = r.Id, DayId = b.DayId, BookingId = b.Id };

                var rooms = (from r in db.Rooms
                             select r);

                var days = (from d in db.Days
                            select d);

                int currentDay = 1;
                int currentRoom = 1;

                Console.WriteLine($"{userName} - Visar alla rum\n\n" + "[F]öregående\tVecka: " + Globals.weekNumber + "\t[N]ästa\t\t\t[B]oka ett rum den valda veckan\t[T]illbaka\n");

                foreach (var room in rooms)
                {
                    Console.Write("\t" + room.RoomName);
                }

                Console.WriteLine();

                foreach (var day in days)
                {
                    Console.Write($"{day.Name}\t");

                    foreach (var room in rooms)
                    {
                        bool occupied = false;
                        foreach (var booking in bookings.Where(x => x.DayId == currentDay))
                        {
                            if (booking.RoomId != currentRoom)
                            {
                                continue;
                            }
                            else if (booking.RoomId == currentRoom)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.Write($"U\t");
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Black;
                                occupied = true;
                                break;
                            }
                        }
                        currentRoom++;
                        if (occupied == false)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write($"L\t");
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Black;

                        }
                    }
                    Console.WriteLine();
                    currentRoom = 1;
                    currentDay++;

                }
            }


            Console.WriteLine();

            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.KeyChar)
            {
                case 'f':
                    Globals.weekNumber--;
                    ViewAllRooms();
                    break;
                case 'n':
                    Globals.weekNumber++;
                    ViewAllRooms();
                    break;
                case 'b':
                    NewBooking();
                    break;
                case 't':
                    Globals.weekNumber = GetIso8601WeekOfYear(DateTime.Now);
                    UserStartPage();
                    break;
                default:
                    WrongInput();
                    break;
            }

            Console.ReadKey(true);
            Console.Clear();
        }
        private static void NewBooking()
        {
            using (var db = new BookingSystemContext())
            {
                var userName = db.Users.Where(x => x.Id == Globals.loggedInId).Select(x => x.Name).FirstOrDefault();
                bool occupied = false;
                if (Globals.weekNumber >= GetIso8601WeekOfYear(DateTime.Now))
                {
                    Console.WriteLine($"Ny bokning\n\n");
                    Console.SetCursorPosition(0, 13);
                    Console.Write("Ange ID för den dag du vill boka: ");
                    ShowAllDays();
                    Console.SetCursorPosition(0, 14);
                    int newDay = int.Parse(Console.ReadLine());
                    Console.SetCursorPosition(0, 16);
                    Console.Write("Ange ID för det rum du vill boka: ");
                    ShowAllRooms();
                    Console.SetCursorPosition(0, 17);
                    int newRoom = int.Parse(Console.ReadLine());
                    foreach (var b in db.Bookings.Where(w => w.Week == Globals.weekNumber).Where(d => d.DayId == newDay))
                    {
                        if (b.RoomId != newRoom)
                        {
                            continue;
                        }
                        else if (b.RoomId == newRoom)
                        {
                            Console.WriteLine("Det rummet är redan bokat den dagen. Var god välj ett annat.\nTryck någonstans för att fortsätta.");
                            occupied = true;
                            Console.ReadKey();
                            ViewAllRooms();
                        }
                    }
                    if (occupied == false)
                    {
                        ViewRoomInfo(newRoom);
                        Console.WriteLine("Boka detta rum?\n[J]a\t[N]ej");
                    }

                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.KeyChar)
                    {
                        case 'j':
                            var newBooking = new Booking
                            {
                                RoomId = newRoom,
                                UserId = Globals.loggedInId,
                                DayId = newDay,
                                Week = Globals.weekNumber
                            };
                            var bookingList = db.Bookings;
                            bookingList.Add(newBooking);
                            db.SaveChanges();

                            Console.WriteLine("Du har nu bokat ett rum! \nTryck någonstans för att fortsätta.");
                            Console.ReadKey();
                            ViewAllRooms();
                            break;
                        case 'n':
                            ViewAllRooms();
                            break;
                        default:
                            WrongInput();
                            break;
                    }
                }
                else if (Globals.weekNumber < GetIso8601WeekOfYear(DateTime.Now))
                {
                    Console.WriteLine("Det går inte att boka den valda veckan.\nTryck någonstans för att fortsätta.");
                    Console.ReadKey();
                    ViewAllRooms();
                }
            }
        }
        private static void ShowAllDays()
        {
            using (var db = new BookingSystemContext())
            {
                int i = 11;
                Console.SetCursorPosition(45, i);
                Console.WriteLine("ID\tDag");
                foreach (var day in db.Days)
                {
                    i++;
                    Console.SetCursorPosition(45, i);
                    Console.WriteLine($"{day.Id}\t{day.Name}");
                }

            }
        }
        private static void ShowAllRooms()
        {
            using (var db = new BookingSystemContext())
            {
                int i = 11;
                Console.SetCursorPosition(60, i);
                Console.WriteLine("ID\tRumsnamn");
                foreach (var room in db.Rooms)
                {
                    i++;
                    Console.SetCursorPosition(60, i);
                    Console.WriteLine($"{room.Id}\t{room.RoomName}");
                }

            }
        }
        private static void ViewRoomInfo(int roomId)
        {
            Console.WriteLine();
            using (var db = new BookingSystemContext())
            {
                var rooms = (from r in db.Rooms
                             where r.Id == roomId
                             select r);

                foreach (var room in rooms)
                {
                    Console.WriteLine($"Rum {room.RoomName} med rums-id {room.Id} har {room.Seats} stolar, {room.ElectricalOutlets} eluttag,\n{(room.Window ? "" : "inga ")}fönster, {(room.Whiteboard ? "" : "ingen ")}whiteboard, och {(room.Projector ? "" : "ingen ")}projektor.");
                }
            }
        } 
        private static void YourBookings()
        {
            Console.Clear();

            using (var db = new BookingSystemContext())
            {
                var userName = db.Users.Where(x => x.Id == Globals.loggedInId).Select(x => x.Name).FirstOrDefault();

                Console.WriteLine($"{userName} - Dina bokningar\n\n" +
                    $"[A]vboka rum\t[T]illbaka\tNuvarande vecka: {Globals.weekNumber}\n");

                var bookings = (from b in db.Bookings
                                join d in db.Days on b.DayId equals d.Id
                                join r in db.Rooms on b.RoomId equals r.Id
                                orderby b.Week, d.Id
                                where b.UserId == Globals.loggedInId
                                select new { BookingId = b.Id, RoomName = r.RoomName, Week = b.Week, Day = d.Name });

                Console.WriteLine($"Boknings-ID\tRumsnamn\tVecka\tDag");
                foreach (var booking in bookings.Where(x => x.Week >= Globals.weekNumber))
                {
                    Console.WriteLine($"{booking.BookingId}\t\t{booking.RoomName}\t\t{booking.Week}\t{booking.Day}");
                }

                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.KeyChar)
                {
                    case 't':
                        UserStartPage();
                        break;
                    case 'a':
                        Console.WriteLine();
                        Console.Write("Skriv in ID på den bokning du vill ta bort: ");
                        int bookingId = int.Parse(Console.ReadLine());

                        var booking = db.Bookings.Where(x => x.Id == bookingId).SingleOrDefault();

                        if (booking != null)
                        {
                            db.Bookings.Remove((Booking)booking);
                            db.SaveChanges();
                        }

                        Console.WriteLine("Rum avbokat! Tryck någonstans för att fortsätta.");
                        Console.ReadKey();
                        YourBookings();

                        break;
                    default:
                        WrongInput();
                        break;
                }
            }
        }

        //―――――――――――――――――――――Admins metoder――――――――――――――――――――――――――――――――――――――――――――――――
        private static void AdminStartPage()
        {
            Console.Clear();
            using (var db = new BookingSystemContext())
            {
                var userName = db.Users.Where(x => x.Id == Globals.loggedInId).Select(x => x.Name).FirstOrDefault();
                Console.WriteLine($"Välkommen administratör {userName}!\n\n" +
                    "0. Logga ut.\n" +
                    "1. Lägg till ett nytt rum.\n" +
                    "2. Se alla bokningar.\n" +
                    "3. Se statistik.");

                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.KeyChar)
                {
                    case '0':
                        StartPage();
                        break;
                    case '1':
                        AddNewRoom();
                        break;
                    case '2':
                        ViewAllBookings();
                        break;
                    case '3':
                        Statistics();
                        break;
                    default:
                        WrongInput();
                        break;
                }
            }
        }
        private static void AddNewRoom()
        {
            Console.WriteLine("Lägg till ett nytt rum\n\n");
            Console.Write("Rumsnamn: ");
            string roomName = Console.ReadLine();
            Console.Write("Antal stolar: ");
            int seats = int.Parse(Console.ReadLine());
            Console.Write("Antal eluttag: ");
            int electricOutlets = int.Parse(Console.ReadLine());

            Console.WriteLine("Finns det fönster? [J]a\t[N]ej");
            bool window = true;
            string yesNo = Console.ReadLine().ToUpper();
            if (yesNo == "J")
            { window = true; }
            else if (yesNo == "N")
            { window = false; }
            else { WrongInput(); }

            Console.WriteLine("Finns det projektor? [J]a\t[N]ej");
            bool projector = true;
            yesNo = Console.ReadLine().ToUpper();
            if (yesNo == "J")
            { projector = true; }
            else if (yesNo == "N")
            { projector = false; }
            else { WrongInput(); }

            Console.WriteLine("Finns det whiteboard? [J]a\t[N]ej");
            bool whiteboard = true;
            yesNo = Console.ReadLine().ToUpper();
            if (yesNo == "J")
            { whiteboard = true; }
            else if (yesNo == "N")
            { whiteboard = false; }
            else { WrongInput(); }
            using (var db = new BookingSystemContext())
            {
                var newRoom = new Room
                {
                    RoomName = roomName,
                    Seats = seats,
                    ElectricalOutlets = electricOutlets,
                    Window = window,
                    Projector = projector,
                    Whiteboard = whiteboard
                };
                var roomList = db.Rooms;
                roomList.Add(newRoom);
                db.SaveChanges();

            }

            Console.WriteLine("Du har nu lagt till ett rum!\nTryck någonstans för att fortsätta.");
            Console.ReadKey();
            AdminStartPage();
        }
        private static void ViewAllBookings()
        {
            Console.Clear();

            using (var db = new BookingSystemContext())
            {
                var userName = db.Users.Where(x => x.Id == Globals.loggedInId).Select(x => x.Name).FirstOrDefault();

                Console.WriteLine($"{userName} - Alla bokningar\n\n" +
                    $"[A]vboka rum\t[T]illbaka\n");

                var bookings = (from b in db.Bookings
                                join u in db.Users on b.UserId equals u.Id
                                join d in db.Days on b.DayId equals d.Id
                                join r in db.Rooms on b.RoomId equals r.Id
                                orderby b.Week, d.Id
                                select new { BookingId = b.Id, RoomName = r.RoomName, Week = b.Week, Day = d.Name, UserId = u.Id, UserName = u.Name });

                Console.WriteLine($"Boknings-ID\tRumsnamn\tVecka\tDag\tAnvändar-Id\tAnvändare");
                foreach (var booking in bookings)
                {
                    Console.WriteLine($"{booking.BookingId}\t\t{booking.RoomName}\t\t{booking.Week}\t{booking.Day}\t{booking.UserId}\t\t{booking.UserName}");
                }

                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.KeyChar)
                {
                    case 't':
                        AdminStartPage();
                        break;
                    case 'a':
                        Console.WriteLine();
                        Console.Write("Skriv in ID på den bokning du vill ta bort: ");
                        int bookingId = int.Parse(Console.ReadLine());

                        var booking = db.Bookings.Where(x => x.Id == bookingId).SingleOrDefault();

                        if (booking != null)
                        {
                            db.Bookings.Remove((Booking)booking);
                            db.SaveChanges();
                        }

                        Console.WriteLine("Rum avbokat! Tryck någonstans för att fortsätta.");
                        Console.ReadKey();
                        ViewAllBookings();

                        break;
                    default:
                        WrongInput();
                        break;
                }
            }
        }
        private static void Statistics()
        {
            Console.WriteLine("\nStatistik\n\n" +
                "0. Gå tillbaka\n" +
                "1. Populäraste rummet\n" +
                "2. Aktivaste användaren");

            using (var db = new BookingSystemContext())
            {

                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.KeyChar)
                {
                    case '0':
                        AdminStartPage();
                        break;
                    case '1':
                        var mostBookedRooms = (from r in db.Rooms
                                               join b in db.Bookings on r.Id equals b.RoomId
                                               select new { r.RoomName, b.RoomId }).ToList().GroupBy(r => r.RoomName);
                        int count = 1;

                        Console.WriteLine("\nTre populäraste rummen:\n");

                        foreach (var room in mostBookedRooms.OrderByDescending(r => r.Count()).Take(3))
                        {
                            Console.WriteLine($"{count}: Rum {room.Key} har bokats {room.Count()} gånger.");
                            count++;
                        }
                        break;
                    case '2':
                        var mostActiveUsers = (from u in db.Users
                                               join b in db.Bookings on u.Id equals b.UserId
                                               select new { u.Name, b.UserId }).ToList().GroupBy(u => u.Name);
                        count = 1;

                        Console.WriteLine("\nTre aktivaste användarna:\n");

                        foreach (var user in mostActiveUsers.OrderByDescending(u => u.Count()).Take(3))
                        {
                            Console.WriteLine($"{count}: {user.Key} har bokat rum {user.Count()} gånger.");
                            count++;
                        }
                        break;
                    default:
                        WrongInput();
                        break;
                }
            }
            Console.ReadKey();
            AdminStartPage();
        }

        //――――――――――――――――――――――Hjälpmetoder―――――――――――――――――――――――――――――――――――――――――――――――――
        internal static void WrongInput()
        {
            Console.WriteLine("Fel inmatning. Försök igen.");
        }
        public static int GetIso8601WeekOfYear(DateTime time) // metod som räknar ut nuvarande veckonummer. snodd direkt från stackoverflow. 
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}


