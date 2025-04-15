using System.Data;
using System.Text.Json.Serialization;
using Api.Helpers;
using Api.Repositories;
using Api.Services;
using Api.Shared;
using Npgsql;
using Microsoft.AspNetCore.Builder;

namespace  Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory()).AddUserSecrets<Program>();
        configurationBuilder.AddJsonFile("appsettings.json").AddEnvironmentVariables();
        configurationBuilder.Build();
        ConfigureServices(builder.Services);
        WebApplication app = ConfigureApp(builder);
        app.Run();            
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient<GoogleAuthService>();
        services.AddSingleton<TokenService>();
        services.AddScoped<GoogleAuthService>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddControllers();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        services.AddOpenApi();
        services.AddScoped<IDbConnection>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("DbConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Database connection string is missing.");
            }

            return new NpgsqlConnection(connectionString);
        });
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IAccountTypeRepository, AccountTypeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<AccountMapper>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserService, UserService>();
    }

    public static WebApplication ConfigureApp(WebApplicationBuilder builder)
    {
        var app = builder.Build();
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
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
    
        
}
