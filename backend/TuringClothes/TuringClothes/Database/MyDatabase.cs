using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;

namespace TuringClothes.Database
{
    public class MyDatabase : DbContext
    {
        private const string DATABASE_PATH = "turing.db";

        //Tablas o entidades de la DB
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<TemporaryOrder> TemporaryOrders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Order> Orders { get; set; }

        //Configura EF para crear un archivo de la base de datos Sqlite
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            string basedir = AppDomain.CurrentDomain.BaseDirectory;
            optionsBuilder.UseSqlite($"Datasource={basedir}{DATABASE_PATH}");

#else   
            string connection = "Server=db10820.databaseasp.net; Database=db10820; Uid=db10820; Pwd=J%z45K+y_6bE;";
            optionsBuilder.UseMySql(connection, ServerVersion.AutoDetect(connection));
#endif
        }
    }
}
