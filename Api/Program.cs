using Api.Services;
using Api.Repositories;
using Microsoft.AspNetCore.Builder;

namespace  Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory()).AddUserSecrets<Program>();
        configurationBuilder.AddJsonFile("appsettings.json").AddEnvironmentVariables();
        var configuration = configurationBuilder.Build();
        ConfigureServices(builder.Services, configuration);
        WebApplication app = ConfigureApp(builder);
        app.Run();            
    }

    public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddHttpClient<GoogleAuthService>();
        services.AddSingleton<TokenService>();
        services.AddScoped<GoogleAuthService>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddControllers();
        services.AddOpenApi();
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
