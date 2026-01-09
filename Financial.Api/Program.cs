using Financial.Api.Common;
using Financial.Api.Endpoints.Import;
using Financial.Api.Infra;
using Microsoft.EntityFrameworkCore;

namespace Financial.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);                       

            // Database and configurations services
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<List<string>>(builder.Configuration.GetSection("CIKs"));

            // Business services
            builder.Services.AddScoped<IImportCompanyService, ImportCompanyService>();
            builder.Services.AddScoped<IListCompanyService, ListCompanyService>();

            // Other required services
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpClient();

            var app = builder.Build();

            app.UseMiddleware<ExceptionMiddleware>();

            // Swagger / OpenApi
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();            

            app.UseHttpsRedirection();            
            
            // Endpoints
            var companyEndpoints = 
                app.MapGroup("/companies")
                   .WithTags("Companies");

            companyEndpoints.MapImportCompanyEndpoint();
            companyEndpoints.MapListCompanyEndpoint();

            // Executes the EF migration and once the apps starts,
            // This is just for making this test evaluation easier, in a real production app this should be replaced by migrations in a CI pipeline
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();                
                db.Database.Migrate();               
            }

            app.Run();
        }
    }
}
