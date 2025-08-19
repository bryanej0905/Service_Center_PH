using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APIServiceCenterPH.Models;
using APIServiceCenterPH.Config;

namespace APIServiceCenterPH.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwt;
        private readonly string _connectionString;

        public AuthController(IConfiguration config)
        {
            _jwt = config.GetSection("Jwt").Get<JwtSettings>();
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto credentials)
        {
            string username = credentials.Username.Trim();
            string password = credentials.Password;

            using var sha = SHA256.Create();
            string passwordHash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));

            string sql = "SELECT COUNT(*) FROM ApiUsers WHERE Username = @user AND PasswordHash = @pass AND EsActivo = 1";
            int count = 0;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@pass", passwordHash);

                await conn.OpenAsync();
                count = (int)await cmd.ExecuteScalarAsync();
            }

            if (count == 0)
                return Unauthorized("Credenciales incorrectas");

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}
