using Microsoft.EntityFrameworkCore;
using MoeStore.Entities.DB;
using MoeStore.Entities.Models;
using MoeStore.Services.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Services.Repository
{
    public class SubjectRepo : ISubjectRepo
    {
        private readonly ApplicationDbContext db;

        public SubjectRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<List<Subject>> GetAllAsync()
        {
            return await db.Subjects.ToListAsync();
        }
    }
}
