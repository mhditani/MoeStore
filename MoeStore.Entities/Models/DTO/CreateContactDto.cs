using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Entities.Models.DTO
{
    public class CreateContactDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Phone, MaxLength(100)]
        public string? Phone { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required, MinLength(20), MaxLength(4000)]
        public string Message { get; set; }
    }
}
