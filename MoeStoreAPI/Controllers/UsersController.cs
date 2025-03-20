using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoeStore.Entities.DB;
using MoeStore.Entities.Models.DTO;
namespace MoeStoreAPI.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext db;

        public UsersController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // allow admin to read available users
        [HttpGet]
        public async Task<IActionResult> GetUsers(int? page)
        {
            // add pagination
            if (page == null || page < 1)
            {
                page = 1;
            }
            int pageSize = 5;
            int totalPages = 0;
            
            decimal count  = await db.Users.CountAsync();
            totalPages = (int)Math.Ceiling(count / pageSize);
            //totalPages = Math.Min(totalPages, pageSize);

            var users = await db.Users
                .OrderByDescending(u => u.Id)
                .Skip((int)(page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            List<UserProfileDto> userProfiles = new List<UserProfileDto>();

            foreach (var user in users)
            {
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

                userProfiles.Add(userProfileDto);
            }

            var response = new
            {
                Users = userProfiles,
                TotalPages = totalPages,
                pageSize = pageSize,
                page = page
            };

            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
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
    }
}
