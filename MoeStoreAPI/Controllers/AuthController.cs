using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MoeStore.Entities.DB;
using MoeStore.Entities.Models;
using MoeStore.Entities.Models.DTO;
using MoeStore.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoeStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext db;
        private readonly EmailSender emailSender;

        public AuthController(IConfiguration configuration, ApplicationDbContext db, EmailSender emailSender)
        {
            this.configuration = configuration;
            this.db = db;
            this.emailSender = emailSender;
        }


        private string CreateJWToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", "" + user.Id),
                new Claim("role", "" + user.Role)
            };

            string secretKey = configuration["JwtSettings:Key"]!;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var token = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            // check if the email is already used or not.
            var emailCount = await db.Users.CountAsync(u => u.Email == userDto.Email);
            if (emailCount > 0)
            {
                ModelState.AddModelError("Email", "This email address is already used");
                return BadRequest(ModelState);
            }

            // otherwise the email isn't used. encrypt the password.
            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

            // create new account
            User user = new User()
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone ?? "",
                Address = userDto.Address,
                Password = encryptedPassword,
                Role = "client",
                CreatedAt = DateTime.Now
            };

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            // create the token
            var jwt = CreateJWToken(user);

            // create DTO to send back to the user
            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = DateTime.Now
            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response);
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("Error", "Email or Password not valid");
                return BadRequest(ModelState);
            }

            // verify the password
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(new User(), user.Password, password);

            // check if the verification is successful or not
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Password", "Wrong Password");
                return BadRequest(ModelState);
            }

            // if verification was successful
            var jwt = CreateJWToken(user);

            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = DateTime.Now
            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response); 
        }


        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }
            // delete any old password reset request
            var oldPasswordReset = await db.PasswordResets.FirstOrDefaultAsync(r => r.Email == email);
            if (oldPasswordReset != null)
            {
                db.Remove(oldPasswordReset);
            }
            // Create new password reset token
            string token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString();

            // save the new password in the PasswordResets Table
            var PasswordReset = new PasswordReset()
            {
                Email = email,
                Token = token,
                CreatedAt = DateTime.Now
            };
            await db.PasswordResets.AddAsync(PasswordReset);
            await db.SaveChangesAsync();

            // send the password reset token to the user by email
            string emailSubject = "Password Reset";
            string username = user.FirstName + " " + user.LastName;
            string emailMessage = "Dear " + username + "\n" +
                "we recieved your password reset request.\n" +
                "Please copy the following token and paste it in the Password Reset Form:\n" +
                token + "\n\n" +
                "Best Regards\n";

            emailSender.SendEmail(emailSubject, email, username, emailMessage).Wait();

            return Ok("Check you mail inbox");
        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string token, string password)
        {
            var passReset = await db.PasswordResets.FirstOrDefaultAsync(r => r.Token == token);
            if (passReset == null)
            {
                ModelState.AddModelError("Token", "Wrong or Expired Token");
                return BadRequest(ModelState);
            }
            // Read the user that have the email address that corresponds to this request
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == passReset.Email);
            if (user == null)
            {
                ModelState.AddModelError("Token", "Wrong or Expired Token");
                return BadRequest(ModelState);
            }

            // encrypt password
            var passwordHasher = new PasswordHasher<User>();
            string encryptedPassword = passwordHasher.HashPassword(new User(), password);

            // save the new encrypted password
            user.Password = encryptedPassword;

            // delete old token from the database
            db.PasswordResets.Remove(passReset);

            await db.SaveChangesAsync();
            return Ok();
        }


        [Authorize]
        [HttpGet("UserProfile")]
        public  async Task<IActionResult> GetUserProfile()
        {
            // call the function
            int id = GetUsrId();
            
            // Read The User From The Database
            var user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return Unauthorized();
            }
            
            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfileDto);
        }


        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(UserProfileUpdateDto userProfileUpdateDto)
        {
            int id = GetUsrId();
            var user = await db.Users.FindAsync(id);
            if (user == null) 
            {
                return Unauthorized();
            }

             // update user profile
             user.FirstName = userProfileUpdateDto.FirstName;
            user.LastName = userProfileUpdateDto.LastName;
            user.Email = userProfileUpdateDto.Email;
            user.Phone = userProfileUpdateDto.Phone ?? "";
            user.Address = userProfileUpdateDto.Address;

            await db.SaveChangesAsync();

            var userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Ok(userProfileDto);
        }


        private int GetUsrId()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return 0;
            }
            // read teh claim
            var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower() == "id");
            if (claim == null)
            {
                return 0;
            }
            // convert the value of claim to integer
            int id;
            try
            {
                id = int.Parse(claim.Value);
            }
            catch (Exception)
            {

                return 0;
            }
            return id;
        }
        //[Authorize]
        //[HttpGet("GetTokenClaims")]
        //public IActionResult GetTokenClaims()
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null)
        //    {
        //        Dictionary<string, string> claims = new Dictionary<string, string>();
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            claims.Add(claim.Type, claim.Value);
        //        }
        //        return Ok(claims);
        //    }
        //    return Ok("Identity is Null");
        //}

        //[Authorize]
        //[HttpGet("AuthorizeAuthenticatedUsers")]
        //public IActionResult AuthorizeAuthenticatedUsers()
        //{
        //    return Ok("Your are Authorized");
        //}

        //[Authorize(Roles = "admin")]
        //[HttpGet("AuthorizeAdmin")]
        //public IActionResult AuthorizeAdmin()
        //{
        //    return Ok("Your are Authorized");
        //}

        //[Authorize(Roles = "admin, seller")]
        //[HttpGet("AuthorizeAdminAndSeller")]
        //public IActionResult AuthorizeAdminAndSeller()
        //{
        //    return Ok("Your are Authorized");
        //}

        //[HttpGet("TestToken")]
        //public IActionResult TestToken()
        //{
        //    User user = new User()
        //    {
        //        Id = 2,
        //        Role = "admin"
        //    };

        //    string jwt = CreateJWToken(user);
        //    var response = new
        //    {
        //        JwtToken = jwt
        //    };
        //    return Ok(response);
        //}
    }

    
}
