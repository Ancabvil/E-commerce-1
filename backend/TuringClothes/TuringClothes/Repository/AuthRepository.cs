using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TuringClothes.Database;

namespace TuringClothes.Repository
{
    public class AuthRepository : Repository<User, int>
    {
        private readonly TokenValidationParameters _tokenParameters;
        public AuthRepository(MyDatabase myDatabase, TokenValidationParameters tokenParameters) : base(myDatabase) 
        {
            _tokenParameters = tokenParameters;
        }

        
        public async Task<User?> GetByEmail(string mail)
        {
           return await GetQueryable().FirstOrDefaultAsync(email => email.Email == mail);

        }
 
        
    }
}
