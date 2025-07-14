using Prensadao.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProducts();
        Task<int> AddProduct(Product product);
    }
}
