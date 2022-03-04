using DataAccess;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;

using Ozytis.Common.Core.Logs.Core;

namespace Web
{
    public class Program
    {

        public static void Main(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args);
         
            builder.ConfigureAppConfiguration((hostContext, config) =>
            {
                if (hostContext.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
            });

            builder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();

                webBuilder.ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    //loggingBuilder.AddConsole();

                    loggingBuilder.AddOzytisLogging(options =>
                    {
                        options.GetConnectionString = services => services.GetService<IConfiguration>().GetConnectionString(Startup.ConnectionStringName);
                        options.TempLogsDirectory = Path.Combine(Environment.CurrentDirectory, "TempLogs");
                        options.SiteName = "Dell-Cagip";
                        options.AutomaticallyCollectLogs = true;
                        options.GetCurrentUrl = (services) =>
                        {
                            IHttpContextAccessor httpContextAccessor = services?.GetService<IHttpContextAccessor>();
                            var req = httpContextAccessor?.HttpContext?.Request;
                            return req != null ? UriHelper.GetDisplayUrl(req) : "Url inconnue";
                        };

                        options.GetCurrentUser ??= (services) =>
                        {
                            IHttpContextAccessor httpContextAccessor = services?.GetService<IHttpContextAccessor>();
                            return (httpContextAccessor?.HttpContext?.User?.Identity?.Name) ?? "Non identifié";
                        };
                    });
                });
            });

            var app = builder.Build();

            app.Run();
        }
    }
}
