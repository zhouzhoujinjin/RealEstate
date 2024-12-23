using Approval_Net8;
using CyberStone.Core;
using Microsoft.EntityFrameworkCore;
using RealEstate.Services;

namespace RealEstate
{
  public static class ServiceExtensions
  {
    public static void AddRealEstateServices(this IServiceCollection services, IConfiguration configuration)
    {
      var connectionString = configuration.GetConnectionString("Default");
      //services.AddDbContext<LetianDbContext>(options =>
      //    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), o => o.MigrationsAssembly("Letian")));
      //services.AddScoped<ConfigManager>();
      //services.AddScoped<DepartmentManager>();
      //services.AddScoped<FileUploadManager>();
      //services.AddScoped<PeopleManager>();
      //services.AddScoped<RobotManager>();
      services.AddScoped<PdfService>();
      services.AddApprovalServices(configuration);
      services.AddCyberStone(configuration);
      //services.AddScoped<TaskManager>();
      //services.AddApprovalHooks(configuration);
    }
  }
}
