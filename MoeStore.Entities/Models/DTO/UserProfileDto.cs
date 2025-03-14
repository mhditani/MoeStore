﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Entities.Models.DTO
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        public string Email { get; set; } = ""; 

        public string Phone { get; set; } = "";

        public string Address { get; set; } = "";
   

        public string Role { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
