
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using TuringClothes.Controllers;
using TuringClothes.Database;
using TuringClothes.Model;
using TuringClothes.Pagination;
using TuringClothes.Repository;
using TuringClothes.Services;
using TuringClothes.Services.Blockchain;
using ReviewService = TuringClothes.Services.ReviewService;

namespace TuringClothes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();


            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    Description = "Escribe **_SOLO_** tu token JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>(true, JwtBearerDefaults.AuthenticationScheme);
            });

            builder.Services.AddAuthentication().AddJwtBearer(options =>
            {
                string key = "bdisub678kji@m32iods/3bjk970kjbhvytdtjvñkpokop";
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

            builder.Services.AddSingleton(provider =>
            provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters
             );

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<EmailService>();

            builder.Services.AddScoped<MyDatabase>();
            builder.Services.AddScoped<AuthRepository>();
            builder.Services.AddScoped<ProductRepository>();
            builder.Services.AddScoped<CartRepository>();
            builder.Services.AddScoped<CatalogService>();
            builder.Services.AddScoped<SmartSearchService>();
            builder.Services.AddScoped<ReviewService>();
            builder.Services.AddScoped<TemporaryOrderRepository>();
            builder.Services.AddScoped<OrderRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<BlockchainService>();
            builder.Services.AddHostedService<MyBackgroundService>();
            builder.Services.AddScoped<UnitOfWork>();


            builder.Services.AddPredictionEnginePool<ModelInput, ModelOutput>()
                .FromFile("TuringModel1.mlnet");

            StripeConfiguration.ApiKey = "sk_test_51QJzkGRqNFmfQiA9GJ41Y9yoGmgUsXLzdV8mbk06Rtfj5Q7d5Z0F0foXulLkkIKeRGpyGy5L4zM6UwtbprVlNbp300qCldYM25";


            if (builder.Environment.IsDevelopment())
            {
                //Permite CORS
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
                });
            }
            var app = builder.Build();

            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) { 
                app.UseSwagger();
                app.UseSwaggerUI();
                //permite CORS
                app.UseCors();
            }
            SeedDatabase(app.Services);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            //crea la base de datos si no está ya creada
            using (IServiceScope scope = app.Services.CreateScope())
            {

                MyDatabase myDatabase = scope.ServiceProvider.GetService<MyDatabase>();
                myDatabase.Database.EnsureCreated();

            }


            InitBackgroundService(app.Services);



            app.Run();
        }

        static void SeedDatabase(IServiceProvider serviceProvider)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            using MyDatabase dataBaseContext = scope.ServiceProvider.GetService<MyDatabase>();

            if (dataBaseContext.Database.EnsureCreated())
            {
                DataSeed seeder = new DataSeed(dataBaseContext);
                seeder.Seed();
            }
        }

        static void InitBackgroundService(IServiceProvider serviceProvider)
        {
            serviceProvider.GetService<MyBackgroundService>();
        }
    }
}
