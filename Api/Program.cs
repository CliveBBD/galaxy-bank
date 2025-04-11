using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Api.Services;

namespace  Api
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Directory.GetCurrentDirectory()).AddUserSecrets<Program>();
            var configuration = configurationBuilder.Build();
            ConfigureServices(builder.Services, configuration);
            var app = builder.Build();
            
            Configure(app);            
        }

        public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {

            // Configure CORS to allow CLI client
            services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", policy =>
                {
                    policy.WithOrigins("http://localhost:*")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddHttpClient<GoogleAuthService>();
            services.AddSingleton<TokenService>();
            services.AddScoped<GoogleAuthService>();

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
                o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options => {
                options.Cookie.Name = "galaxy";
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = "/login";

            })
            .AddGoogleOpenIdConnect(options =>
            {   
                options.ClientId = configuration["Authentication:Google:ClientId"];
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                // options.CallbackPath = ;
            });

            services.AddControllers();
            services.AddOpenApi();
        }

        public static void Configure(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUi(options =>
                {
                    options.DocumentPath = "/openapi/v1.json";
                });
            }

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
      
            
    }
    
}
