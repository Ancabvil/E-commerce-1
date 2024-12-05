using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TuringClothes.Database;

namespace TuringClothes.Repository
{
    public class ProductRepository : Repository<Product, long>
    {
        private readonly MyDatabase _myDataBase;
        public ProductRepository(MyDatabase myDatabase) : base(myDatabase) 
        {
            _myDataBase = myDatabase;
        }



        public async Task<Product?> GetProductById(long id)
        {
            return await _myDataBase.Products.Include(p => p.Reviews).ThenInclude(r => r.User).FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Product?> GetProductByIdOrder(long id)
        {
            return await _myDataBase.Products.FirstOrDefaultAsync(n => n.Id == id);
        }
    }
}
