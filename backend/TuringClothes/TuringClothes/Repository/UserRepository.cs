using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuringClothes.Database;

namespace TuringClothes.Repository
{
    public class UserRepository : Repository<User, long>
    {
        private readonly MyDatabase _myDatabase;
        public UserRepository(MyDatabase myDatabase) : base(myDatabase)
        {
            _myDatabase = myDatabase;
        }

        public async Task<User> GetUserById(long userId)
        {
            return _myDatabase.Users.FirstOrDefault(u => u.Id == userId);
        }

        public async Task AddAsync(User user)
        {
            await _myDatabase.AddAsync(user);
        }

        public async Task<List<Order>> GetordersByUser(long userId)
        {
            var user = await GetByIdAsync(userId);
            var orders = user.Orders.ToList();
            return orders;
        }

    }
}
