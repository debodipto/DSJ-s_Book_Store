using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DSJsBookStore.Data;

namespace CheckAdmin
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Data Source=../bookshop.db";

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connectionString)
                .Options;

            using var context = new ApplicationDbContext(options);

            Console.WriteLine("Checking database for users and roles...");

            var users = context.Users.ToList();
            Console.WriteLine($"Total users: {users.Count}");

            foreach (var user in users)
            {
                Console.WriteLine($"User: {user.Email} - {user.UserName}");

                var userRoles = context.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.FirstOrDefault(r => r.Id == userRole.RoleId);
                    if (role != null)
                    {
                        Console.WriteLine($"  Role: {role.Name}");
                    }
                }
            }

            var roles = context.Roles.ToList();
            Console.WriteLine($"Total roles: {roles.Count}");
            foreach (var role in roles)
            {
                Console.WriteLine($"Role: {role.Name}");
            }
        }
    }
}
