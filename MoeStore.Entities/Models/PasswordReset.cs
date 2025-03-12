using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Entities.Models
{
    [Index("Email", IsUnique = true)]
    public class PasswordReset
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Email { get; set; } = "";
        [MaxLength(100)]
        public string Token { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
