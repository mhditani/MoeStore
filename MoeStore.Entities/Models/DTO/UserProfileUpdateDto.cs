using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Entities.Models.DTO
{
    public class UserProfileUpdateDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required, EmailAddress, MaxLength(100)]

        public string Email { get; set; } = "";
        [MaxLength(20)]

        public string? Phone { get; set; }
        [Required, MaxLength(100)]

        public string Address { get; set; } = "";
    }
}
