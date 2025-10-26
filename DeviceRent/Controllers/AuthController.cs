using DeviceRent;
using DeviceRent.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DeviceRent.Data;
using DeviceRent.Models;

namespace DeviceRent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AuthController(ApplicationContext context)
        {
            _context = context;
        }

        // Авторизация (вход)
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);
            if (user == null)
                return Unauthorized(new { message = "Неверный логин или пароль" });

            var token = JwtHelper.GenerateToken(user);
            return Ok(new { token });
        }

        // Регистрация обычного пользователя
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            // Проверка существующего пользователя
            if (_context.Users.Any(u => u.Login == model.Login))
            {
                return BadRequest(new { message = "Аккаунт с таким логином уже существует" });
            }

            var user = new User
            {
                Login = model.Login,
                Password = model.Password,
                Role = model.Role ?? "user"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Аккаунт успешно зарегистрирован", user });
        }

        // Регистрация администратора
        [Authorize(Roles = "admin")]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            if (_context.Users.Any(u => u.Login == model.Login))
            {
                return BadRequest(new { message = "Аккаунт с таким логином уже существует" });
            }

            var user = new User
            {
                Login = model.Login,
                Password = model.Password,
                Role = model.Role ?? "admin"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Аккаунт администратора успешно зарегистрирован", user });
        }

        // Получение списка всех пользователей
        [Authorize(Roles = "admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Login, u.Password, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        // Получение пользователя по ID
        [Authorize(Roles = "admin")]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(new { user.Id, user.Login, user.Role });
        }

        // ✏️ Обновление пользователя
        [Authorize(Roles = "admin")]
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] RegisterModel model)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Login = model.Login;
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = model.Password;
            user.Role = model.Role ?? user.Role;

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        // Удаление пользователя
        [Authorize(Roles = "admin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [Authorize]
        [HttpGet("current-user")]
        public IActionResult GetCurrentUser()
        {
            // Получаем имя пользователя из claims
            var userName = User.Identity?.Name;

            // Получаем другие claims если нужно
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User information not available");
            }

            return Ok(new
            {
                username = userName,
                userId = userId,
                role = userRole
            });
        }
    }

    // DTO-модели
    public class LoginModel
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class RegisterModel
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}

