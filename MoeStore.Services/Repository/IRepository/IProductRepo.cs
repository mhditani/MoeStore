using MoeStore.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Services.Repository.IRepository
{
    public interface IProductRepo
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdtadeProductAsync(int id, Product product);
        Task<Product?> DeleteAsync(int id);
    }
}
