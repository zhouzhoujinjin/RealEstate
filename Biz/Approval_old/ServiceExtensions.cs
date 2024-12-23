using Approval.Managers;
using Approval.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PureCode;
using SeafileClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval
{
  public static class ServiceExtensions
  {
    public static void AddApprovalServices(this IServiceCollection services, IConfiguration configuration)
    {
      services.AddDbContext<ApprovalDbContext>(options =>
          options.UseMySql(configuration.GetConnectionString("Default"), o => o.MigrationsAssembly("Letian")));

      services.AddRazorPages();
      services.AddScoped<ApprovalFlowManager>();
      services.AddScoped<ApprovalManager>();
      services.AddScoped<FileManager>();
      services.AddScoped<TemplateManager>();
      services.AddScoped<SeafileManager>();


      var assembly = typeof(Controllers.OnlyOfficeController).Assembly;

      //Create an EmbeddedFileProvider for that assembly
      var embeddedFileProvider = new EmbeddedFileProvider(
          assembly,
          "Approval"
      );

      services.Configure<SeafileOptions>(configuration.GetSection("SeafileOptions"));

      services.AddScoped(x =>
      {
        var api = SeafileApi.GetApi(configuration.GetValue<string>("SeafileOptions:BaseUrl"));
        return api;
      });
    }
  }
}
