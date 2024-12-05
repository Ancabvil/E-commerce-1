using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TuringClothes.Database;
using TuringClothes.Dtos;
using TuringClothes.Model;
using TuringClothes.Repository;

namespace TuringClothes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly TokenValidationParameters _tokenParameters;
        private readonly UnitOfWork _unitOfWork;
   
        public AuthController(UnitOfWork unitOfWork, IOptionsMonitor<JwtBearerOptions> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _tokenParameters = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
        }

        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto loginData)
        {
            var user = await _unitOfWork.AuthRepository.GetByEmail(loginData.Email);

            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }
            if (loginData.Email == user.Email && BCrypt.Net.BCrypt.Verify(loginData.Password, user.Password))
            {
               string stringToken = GenerateToken(user);

                return Ok(new AuthResultDto { AccessToken = stringToken });
            }
            return Unauthorized("Email o contraseña incorrecto");
        }


        [HttpPost("Register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerData)
        {
            var existingUser = await _unitOfWork.AuthRepository.GetByEmail(registerData.Email);
            if (existingUser != null)
            {
                return Conflict("El correo electrónico ya está registrado.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerData.Password);
            var newUser = new User
            {
                Name = registerData.Name,
                Surname = registerData.Surname,
                Email = registerData.Email,
                Password = hashedPassword,
                Address = registerData.Address,
                Role = "user"
            };
            await _unitOfWork.UserRepository.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();
            string stringToken = GenerateToken(newUser);

            return Ok(new AuthResultDto { AccessToken = stringToken });
        }


        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult GetSecret()
        {
            return Ok(new { message = "Esto es un secreto que nadie puede saber..." });
        }

        [HttpGet("users")]
        public IActionResult Users()
        {
            return Ok(new { message = "Esto es un secreto que nadie puede saber..." });
        }


        [HttpGet("user by email")]
        public async Task<User?> GetUserByEmail(string mail)
        {
            var user = await _unitOfWork.AuthRepository.GetByEmail(mail);
            return user;
        }

        private string GenerateToken(User user)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                    {
                        { "id", user.Id },
                        { ClaimTypes.Name, user.Name},
                        { ClaimTypes.Email, user.Email },
                        { ClaimTypes.Role, user.Role }

                    },
                Expires = DateTime.UtcNow.AddYears(1),
                SigningCredentials = new SigningCredentials(
                       _tokenParameters.IssuerSigningKey,
                       SecurityAlgorithms.HmacSha256Signature)
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }

    }



}

