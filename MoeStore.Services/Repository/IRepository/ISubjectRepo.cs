using MoeStore.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Services.Repository.IRepository
{
    public interface ISubjectRepo
    {
        Task<List<Subject>> GetAllAsync();
    }
}
