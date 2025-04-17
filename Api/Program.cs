namespace  Api;

using System.Data;
using System.Text.Json.Serialization;
using Api.Helpers;
using Api.Repositories;
using Api.Services;
using Npgsql;
using Api.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public class Program
{
    public static void Main(string[] args)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Configuration.AddEnvironmentVariables();
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
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = "https://accounts.google.com";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://accounts.google.com",
                ValidateAudience = true,
                ValidAudiences = new[] {"438794123703-9aqbuhmv0asuhr074hqd5o2lf7c7rpap.apps.googleusercontent.com"}, // Important!
                ValidateLifetime = true
            };
        });

        services.AddAuthorization();
        services.AddCors();
        services.AddHttpClient<GoogleAuthService>();
        services.AddSingleton<TokenService>();
        services.AddScoped<GoogleAuthService>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddControllers();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
        services.AddOpenApi();
        services.AddScoped<IDbConnection>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = Constants.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Database connection string is missing.");
            }

            return new NpgsqlConnection("Host=localhost,Port=5432;Database=galaxy-bank-local;Username=postgres;Password=postgres;");
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
        services.AddScoped<IDepositRepository, DepositRepository>();
        services.AddScoped<IWithdrawRepository, WithdrawRepository>();
        services.AddScoped<ITransactionTypeRepository, TransactionTypeRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionReferenceRepository, TransactionReferenceRepository>();
        services.AddScoped<IDepositService, DepositService>();
        services.AddScoped<IWithdrawService, WithdrawService>();
        services.AddScoped<ITransactionTypeService, TransactionTypeService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ITransactionReferenceService, TransactionReferenceService>();
        services.AddScoped<IDisputeRepository, DisputeRepository>();
        services.AddScoped<IDisputeService, DisputeService>();
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

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();
        app.MapControllers();
        
        return app;
    }
    
        
}
