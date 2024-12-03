using Microsoft.AspNetCore.Identity;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MySql.Data.MySqlClient;

namespace backend1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var fallbackString = builder.Configuration.GetConnectionString("FallbackConnection");

            System.Threading.Thread.Sleep(5000);


            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    connection.Close();
                    Console.WriteLine("Using default database");
					//create the db context.
					builder.Services.AddDbContext<ApplicationDbContext>(options =>
					{
						options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
					});
                }
            }
            catch (Exception ex)
            {
				
                Console.WriteLine(ex.ToString()); //Wil writeout unable to connect error if mysql container is not up.
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseMySql(fallbackString, ServerVersion.AutoDetect(fallbackString));
                });
                Console.WriteLine("Using fallback database");
            }


            builder.Services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                    policy => policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("Authorization", "X-Custom-Header")
                );
            });

            builder.Services.AddAuthorization();
            builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add Identity services
            //builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				db.Database.Migrate();
			}

            app.MapIdentityApi<ApplicationUser>()
                .RequireCors(MyAllowSpecificOrigins);


            app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager) =>
            {

                await signInManager.SignOutAsync();
                return Results.Ok();

            }).RequireAuthorization();


            app.MapGet("/pingauth", (ClaimsPrincipal user) =>
            {
                var email = user.FindFirstValue(ClaimTypes.Email); // get the user's email from the claim
                return Results.Json(new { Email = email }); ; // return the email as a plain text response
            }).RequireAuthorization();

            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment())
            // {
                app.UseSwagger();
                app.UseSwaggerUI();
            // }

            app.UseHttpsRedirection();
            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
