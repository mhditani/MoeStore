using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Entities.Models.DTO
{
    public class ProductDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(100)]

        public string Brand { get; set; } = "";
        [Required, MaxLength(100)]

        public string Category { get; set; } = "";
        [Required]
        public decimal Price { get; set; }
        [MaxLength(4000)]
        public string? Description { get; set; } 
        [MaxLength(100)]

        public IFormFile? ImageFile { get; set; } 
    }
}
