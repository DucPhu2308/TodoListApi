using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TodoListApi.DTOs;
using TodoListApi.Models;

namespace TodoListApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly TodoListContext _context;
        private IConfiguration _config;
        private readonly IDistributedCache _cache;

        public UserController(TodoListContext context, IConfiguration config, IDistributedCache cache)
        {
            _context = context;
            _config = config;
            _cache = cache;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login([FromBody] LoginDto user)
        {
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (userInDb == null)
            {
                return NotFound("Tài khoản không tồn tại");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.Password, userInDb.Password))
            {
                return Unauthorized("Mật khẩu không khớp");
            }
            return Ok(new { token = GeneratJwtToken(userInDb) });
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register([FromBody] RegisterDto user)
        {
            // check unique email
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (userInDb != null)
            {
                return BadRequest("Email is already taken");
            }
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<RegisterDto, User>());
            var mapper = new Mapper(mapperConfig);
            var userEntity = mapper.Map<User>(user);
            userEntity.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Login), new { email = userEntity.Email }, userEntity);
        }

        [HttpGet("Profile")]
        [Authorize]
        public async Task<ActionResult<object>> Profile()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            // check cache
            var cacheKey = $"Profile-{email}";
            var cacheProfile = await _cache.GetStringAsync(cacheKey);
            if (cacheProfile != null)
            {
                return Ok(JsonSerializer.Deserialize<object>(cacheProfile));
            }

            var user = await _context.Users
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    Role = u.Role.Name
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }
            // set cache
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return user;
        }

        private string GeneratJwtToken(User user)
        {
            var authClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var token = new JwtSecurityToken(
                               issuer: _config["Jwt:Issuer"],
                               audience: _config["Jwt:Audience"],
                               expires: DateTime.Now.AddHours(3),
                               claims: authClaims,
                               signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
