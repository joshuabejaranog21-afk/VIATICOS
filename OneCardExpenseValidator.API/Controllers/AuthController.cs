using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneCardExpenseValidator.Infrastructure.Data;
using OneCardExpenseValidator.Infrastructure.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OneCardExpenseValidator.API.Controllers;

[Route("Auth")]
public class AuthController : Controller
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("Login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _context.Users
            .Include(u => u.UserRoleUsers)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

        if (user == null || user.IsActive != true)
        {
            TempData["ErrorMessage"] = "Credenciales inválidas o usuario desactivado.";
            return View();
        }

        if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        {
            TempData["ErrorMessage"] = "Credenciales inválidas.";
            return View();
        }

        // Actualizar último login
        user.LastLogin = DateTime.Now;
        await _context.SaveChangesAsync();

        // Crear claims para la autenticación
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FullName", $"{user.FirstName} {user.LastName}")
        };

        // Agregar el employeeId si existe
        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));
        }

        // Agregar roles
        foreach (var userRole in user.UserRoleUsers)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.RoleName));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        TempData["SuccessMessage"] = $"Bienvenido {user.FirstName} {user.LastName}!";

        // Redirigir según el rol
        var roleName = user.UserRoleUsers.FirstOrDefault()?.Role.RoleName;
        if (roleName == "Admin")
        {
            return RedirectToAction("Index", "Home");
        }
        else
        {
            return RedirectToAction("EmployeeDashboard", "Home");
        }
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {
        ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");
        return View();
    }

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword,
        string firstName, string lastName, int? employeeId)
    {
        if (password != confirmPassword)
        {
            TempData["ErrorMessage"] = "Las contraseñas no coinciden.";
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");
            return View();
        }

        if (await _context.Users.AnyAsync(u => u.Username == username))
        {
            TempData["ErrorMessage"] = "El nombre de usuario ya existe.";
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");
            return View();
        }

        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            TempData["ErrorMessage"] = "El email ya está registrado.";
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.IsActive == true), "EmployeeId", "FirstName");
            return View();
        }

        var (passwordHash, passwordSalt) = CreatePasswordHash(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            FirstName = firstName,
            LastName = lastName,
            EmployeeId = employeeId,
            IsActive = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Asignar rol de Empleado por defecto
        var empleadoRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Empleado");
        if (empleadoRole != null)
        {
            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = empleadoRole.RoleId,
                AssignedAt = DateTime.Now
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Usuario registrado exitosamente. Por favor inicie sesión.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Sesión cerrada exitosamente.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("AccessDenied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private (string hash, string salt) CreatePasswordHash(string password)
    {
        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return (hash, salt);
    }

    private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
    {
        var saltBytes = Convert.FromBase64String(storedSalt);
        using var hmac = new HMACSHA512(saltBytes);
        var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        return computedHash == storedHash;
    }
}
