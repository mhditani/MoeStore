using MoeStore.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Services.Repository.IRepository
{
    
    public interface IContactRepo
    {
        Task<List<Contact>> GetAllAsync();

        Task<Contact?> GetByIdAsyn(int id);

        Task<Contact> CreateAsync(Contact contact); 

        Task<Contact?> UpdateAsync(int id,Contact contact);

        Task<Contact?> DeleteAsync(int id);
    }
}
