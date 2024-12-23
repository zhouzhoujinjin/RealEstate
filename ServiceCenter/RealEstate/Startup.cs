using Approval_Net8;
using Hangfire;
using Hangfire.MySql;
using Microsoft.IdentityModel.Logging;
using RealEstate.Models;
using System.Text;
using System.Transactions;

namespace RealEstate
{
  public class Startup
  {
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
      Configuration = configuration;
      _environment = environment;
    }

    public IConfiguration Configuration { get; }
    private bool enableHangfire;

    private IWebHostEnvironment _environment;
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddHttpClient();
      //services.AddPureCode(Configuration);
      services.AddRealEstateServices(Configuration);
      //services.AddApprovalServices(Configuration);

      //services.AddWeChat(Configuration.GetSection("WeChatOptions")).WithWork().WithRedis();
      services.Configure<WorkDomainOptions>(Configuration.GetSection("WorkDomainOptions"));

      services.Configure<ActiveDirectoryOptions>(c =>
      {
        
          c.Port = Configuration.GetSection("ActiveDirectoryOptions:Port").Get<int>();

          c.Zone = Configuration.GetSection("ActiveDirectoryOptions:Zone").Value;
          c.Domain = Configuration.GetSection("ActiveDirectoryOptions:Domain").Value;
          c.Subdomain = Configuration.GetSection("ActiveDirectoryOptions:Subdomain").Value;

          c.Username = Configuration.GetSection("ActiveDirectoryOptions:Username").Value;
          c.Password = Configuration.GetSection("ActiveDirectoryOptions:Password").Value;

          // connection string with port doesn't work on GNU/Linux and Mac OS
          //c.LDAPserver = $"{c.Subdomain}.{c.Domain}.{c.Zone}:{c.Port}";
          c.LDAPserver = $"{c.Subdomain}.{c.Domain}.{c.Zone}";
          // that depends on how it is in your LDAP server
          //c.LDAPQueryBase = $"DC={c.Subdomain},DC={c.Domain},DC={c.Zone}";
          c.LDAPQueryBase = $"DC={c.Domain},DC={c.Zone}";

          c.Crew = new StringBuilder()
              .Append($"CN={Configuration.GetSection("AD:Crew").Value},")
              // check which CN (Users or Groups) your LDAP server has the groups in
              .Append($"CN=Users,{c.LDAPQueryBase}")
              .ToString();
          c.Managers = new StringBuilder()
              .Append($"CN={Configuration.GetSection("AD:Managers").Value},")
              // check which CN (Users or Groups) your LDAP server has the groups in
              .Append($"CN=Users,{c.LDAPQueryBase}")
              .ToString();
        
      });


      var connection = Configuration.GetConnectionString("Default");
      enableHangfire = Configuration.GetValue<bool>("EnableHangfire");
      if (enableHangfire)
      {
        //配置Hangfire
        var cs = Configuration.GetConnectionString("Hangfire");
        //services.AddHostedService<ApprovalBackgroundService>();
        services.AddHangfire(x => x.UseStorage(new MySqlStorage(
                              cs,
                              new MySqlStorageOptions
                              {
                                TransactionIsolationLevel = IsolationLevel.ReadCommitted, // 事务隔离级别。默认是读取已提交。
                              QueuePollInterval = TimeSpan.FromSeconds(15),             //- 作业队列轮询间隔。默认值为15秒。
                              JobExpirationCheckInterval = TimeSpan.FromHours(1),       //- 作业到期检查间隔（管理过期记录）。默认值为1小时。
                              CountersAggregateInterval = TimeSpan.FromMinutes(5),      //- 聚合计数器的间隔。默认为5分钟。
                              PrepareSchemaIfNecessary = true,                          //- 如果设置为true，则创建数据库表。默认是true。
                              DashboardJobListLimit = 50000,                            //- 仪表板作业列表限制。默认值为50000。
                              TransactionTimeout = TimeSpan.FromMinutes(1),             //- 交易超时。默认为1分钟。
                              TablesPrefix = "Hangfire"
                              }
                              )));
        services.AddHangfireServer();
      }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
      if (env.IsDevelopment())
      {
        IdentityModelEventSource.ShowPII = true;
        app.UseDeveloperExceptionPage();
      }

      app.UseDefaultFiles();
      app.UseStaticFiles();
      //app.UseWeChat().WithWork();
      //app.UsePureCode();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapFallbackToFile("/index.html");
        if (enableHangfire)
        {
          endpoints.MapHangfireDashboard();
        }
      });

    }

  }
}