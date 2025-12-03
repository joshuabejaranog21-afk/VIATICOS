using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Entities;
using System.Security.Cryptography;
using System.Text;

namespace OneCardExpenseValidator.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context)
    {
        // Asegurarse de que la base de datos existe
        await context.Database.EnsureCreatedAsync();

        // Verificar si ya existe el rol Admin
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                RoleName = "Admin",
                Description = "Administrador del sistema",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync();
        }

        // Verificar si ya existe el rol Empleado
        var empleadoRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Empleado");
        if (empleadoRole == null)
        {
            empleadoRole = new Role
            {
                RoleName = "Empleado",
                Description = "Empleado regular",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            context.Roles.Add(empleadoRole);
            await context.SaveChangesAsync();
        }

        // Verificar si ya existe un usuario admin
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var (passwordHash, passwordSalt) = CreatePasswordHash("Admin123!");

        if (adminUser == null)
        {
            // Crear nuevo usuario admin
            adminUser = new User
            {
                Username = "admin",
                Email = "admin@onecard.com",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                FirstName = "Admin",
                LastName = "System",
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Asignar rol de Admin
            var userRole = new UserRole
            {
                UserId = adminUser.UserId,
                RoleId = adminRole.RoleId,
                AssignedAt = DateTime.Now
            };
            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            Console.WriteLine("✓ Usuario admin creado exitosamente");
            Console.WriteLine("  Username: admin");
            Console.WriteLine("  Password: Admin123!");
        }
        else
        {
            // Actualizar credenciales si el usuario ya existe
            adminUser.PasswordHash = passwordHash;
            adminUser.PasswordSalt = passwordSalt;
            adminUser.Email = "admin@onecard.com";
            adminUser.IsActive = true;
            adminUser.UpdatedAt = DateTime.Now;

            context.Users.Update(adminUser);
            await context.SaveChangesAsync();

            // Verificar que tenga el rol de Admin
            var hasAdminRole = await context.UserRoles
                .AnyAsync(ur => ur.UserId == adminUser.UserId && ur.RoleId == adminRole.RoleId);

            if (!hasAdminRole)
            {
                var userRole = new UserRole
                {
                    UserId = adminUser.UserId,
                    RoleId = adminRole.RoleId,
                    AssignedAt = DateTime.Now
                };
                context.UserRoles.Add(userRole);
                await context.SaveChangesAsync();
            }

            Console.WriteLine("✓ Usuario admin actualizado exitosamente");
            Console.WriteLine("  Username: admin");
            Console.WriteLine("  Password: Admin123!");
        }

        // Crear usuario empleado de prueba
        var empleadoUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "empleado");
        var (empPasswordHash, empPasswordSalt) = CreatePasswordHash("Empleado123!");

        if (empleadoUser == null)
        {
            // Crear nuevo usuario empleado
            empleadoUser = new User
            {
                Username = "empleado",
                Email = "empleado@onecard.com",
                PasswordHash = empPasswordHash,
                PasswordSalt = empPasswordSalt,
                FirstName = "Juan",
                LastName = "Pérez",
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            context.Users.Add(empleadoUser);
            await context.SaveChangesAsync();

            // Asignar rol de Empleado
            var userRole = new UserRole
            {
                UserId = empleadoUser.UserId,
                RoleId = empleadoRole.RoleId,
                AssignedAt = DateTime.Now
            };
            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            Console.WriteLine("✓ Usuario empleado creado exitosamente");
            Console.WriteLine("  Username: empleado");
            Console.WriteLine("  Password: Empleado123!");
        }
        else
        {
            // Actualizar credenciales si el usuario ya existe
            empleadoUser.PasswordHash = empPasswordHash;
            empleadoUser.PasswordSalt = empPasswordSalt;
            empleadoUser.Email = "empleado@onecard.com";
            empleadoUser.IsActive = true;
            empleadoUser.UpdatedAt = DateTime.Now;

            context.Users.Update(empleadoUser);
            await context.SaveChangesAsync();

            // Verificar que tenga el rol de Empleado
            var hasEmpleadoRole = await context.UserRoles
                .AnyAsync(ur => ur.UserId == empleadoUser.UserId && ur.RoleId == empleadoRole.RoleId);

            if (!hasEmpleadoRole)
            {
                var userRole = new UserRole
                {
                    UserId = empleadoUser.UserId,
                    RoleId = empleadoRole.RoleId,
                    AssignedAt = DateTime.Now
                };
                context.UserRoles.Add(userRole);
                await context.SaveChangesAsync();
            }

            Console.WriteLine("✓ Usuario empleado actualizado exitosamente");
            Console.WriteLine("  Username: empleado");
            Console.WriteLine("  Password: Empleado123!");
        }
    }

    private static (string hash, string salt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return (hash, salt);
    }
}
