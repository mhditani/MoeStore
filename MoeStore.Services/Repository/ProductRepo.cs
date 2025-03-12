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
    public class ProductRepo : IProductRepo
    {
        private readonly ApplicationDbContext db;

        public ProductRepo(ApplicationDbContext db)
        {
            this.db = db;
        }


        public async Task<Product> CreateProductAsync(Product product)
        {
            await db.AddAsync(product);
            await db.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> DeleteAsync(int id)
        {
            var existingProduct = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existingProduct == null)
            {
                return null;
            }
            db.Remove(existingProduct);
            await db.SaveChangesAsync();
            return existingProduct;
        }

        public async Task<(List<Product>, int)> GetAllAsync(string? search, string? sortBy, bool desc, int page = 1, int pageSize = 10)
        {
            var query = db.Products.AsQueryable();

            // Search functionality
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) ||
                                         p.Brand.Contains(search) ||
                                         p.Category.Contains(search));
            }

            // Sorting functionality
            query = sortBy?.ToLower() switch
            {
                "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "category" => desc ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
                _ => desc ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
            };

            // Pagination functionality
            int totalItems = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalItems);
        }
       

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product?> UpdtadeProductAsync(int id, Product product)
        {
            var existingProduct = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = product.Name;
            existingProduct.Brand = product.Brand;
            existingProduct.Category = product.Category;
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.ImageFileName = product.ImageFileName;

            await db.SaveChangesAsync();
            return existingProduct;
        }
    }
}
