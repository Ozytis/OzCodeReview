using Common;

using DataAccess;

using Entities;

using Hangfire;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Ozytis.Common.Core.Emails.Core;
using Ozytis.Common.Core.Emails.NetCore;
using Ozytis.Common.Core.Logs.Core;
using Ozytis.Common.Core.RazorTemplating;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Web
{
    public class Startup
    {
        public const string ConnectionStringName = "DefaultConnection";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection"), o =>
                {
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                });

                //options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
            }, ServiceLifetime.Scoped);

            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.MaxDepth = 10;
                options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            });

            services.AddMvc().AddOzytisMails(new MailConfiguration
            {
                DefaultSenderEmail = this.Configuration["Data:Emails:DefaultSenderEmail"],
                DefaultSenderName = this.Configuration["Data:Emails:DefaultSenderName"],
                HostName = this.Configuration["Data:Emails:SmtpHost"],
                Password = this.Configuration["Data:Emails:SmtpPassword"],
                UserName = this.Configuration["Data:Emails:SmtpUser"],
                Port = int.Parse(this.Configuration["Data:Emails:SmtpPort"]),
                DataBaseConnectionString = this.Configuration.GetConnectionString(Startup.ConnectionStringName),
                UseSsl = false,
                MailDashBoardAllowedRole = UserRoleNames.Administrator
            });

            this.ConfigureSecurity(services);

            foreach (var manager in typeof(Business.UsersManager).Assembly.GetTypes().Where(t => t.Name.EndsWith("Manager")))
            {
                services.AddScoped(manager);
            }

            services.AddHangfire((config) =>
            {
                config.UseSqlServerStorage(this.Configuration.GetConnectionString(Startup.ConnectionStringName));
            }).AddHangfireServer();

            DynamicRazorEngine dynamicRazorEngine = new DynamicRazorEngine();
            dynamicRazorEngine.AddMetadataReference(typeof(IdentityUser).Assembly);
            dynamicRazorEngine.AddMetadataReference(typeof(Entities.ApplicationUser).Assembly);

            Assembly businessAssembly = typeof(Business.UsersManager).Assembly;
            dynamicRazorEngine.LoadAssembly(businessAssembly);

            services.AddSingleton<DynamicRazorEngine>(dynamicRazorEngine);


            services.AddScoped<Setup>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Setup setup, OzLoggerProvider loggerProvider)
        {
            app.UseRouting();
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();

            app.UseStaticFiles(new StaticFileOptions { ServeUnknownFileTypes = true });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHangfireDashboard(options: new DashboardOptions()
            {
                Authorization = new[] {
                    new BasicAuthAuthorizationFilter {
                        User = Configuration["Hangfire:User"],
                        Password = Configuration["Hangfire:Password"]
                    }
                }
            });

            app.UseOzytisMails();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

            setup.Init();
        }

        public void ConfigureSecurity(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
            })
               .AddRoles<IdentityRole>()
               .AddDefaultTokenProviders()
               .AddEntityFrameworkStores<DataContext>();

            this.ConfigureAuthentication(services);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.UserIsAdministrator, policy => policy.RequireRole(UserRoleNames.Administrator));
            });
        }

        public void ConfigureAuthentication(IServiceCollection services)
        {
            byte[] key = Encoding.ASCII.GetBytes(this.Configuration["Authentication:AuthenticationTokenSecretSigningKey"]);

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddSingleton<TokenValidationParameters>(tokenValidationParameters);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    },

                    OnForbidden = ctx =>
                    {
                        ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = async context =>
                    {
                        if (!context.Request.Query.ContainsKey("access_token"))
                        {
                            return;
                        }

                        var token = context.Request.Query["access_token"];

                        if (string.IsNullOrWhiteSpace(token))
                        {
                            context.Fail("Token vide");
                            return;
                        }

                        context.Token = token;
                        await Task.CompletedTask;
                    }
                };
            });
        }
    }
}
