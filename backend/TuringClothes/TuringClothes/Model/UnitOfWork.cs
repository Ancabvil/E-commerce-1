using Microsoft.IdentityModel.Tokens;
using TuringClothes.Database;
using TuringClothes.Repository;

namespace TuringClothes.Model
{
    public class UnitOfWork
    {
        private readonly MyDatabase _database; // NO TENGO CLARO SI DEBERÍA DEJARLA PRIVADA O PÚBLICA PARA USARLA EN OTROS FICHEROS.
        private UserRepository _userRepository;
        private ProductRepository _productRepository;
        private CartRepository _cartRepository;
        private AuthRepository _authRepository;
        private OrderRepository _orderRepository;
        private TemporaryOrderRepository _temporaryOrderRepository;
        private readonly TokenValidationParameters _tokenParameters;

        public UserRepository UserRepository => _userRepository ??= new UserRepository(_database);
        public ProductRepository ProductRepository => _productRepository ??= new ProductRepository(_database);
        public CartRepository CartRepository => _cartRepository ??= new CartRepository(_database);
              
        public AuthRepository AuthRepository=> _authRepository ??= new AuthRepository(_database, _tokenParameters);
       
        public OrderRepository OrderRepository=> _orderRepository ??= new OrderRepository(_database);
        public TemporaryOrderRepository TemporaryOrderRepository => _temporaryOrderRepository ??= new TemporaryOrderRepository(_database);


        public UnitOfWork(MyDatabase database)
        {
            _database = database;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _database.SaveChangesAsync() > 0;
        }
    }
}
