using API.Services;
using Application.Interfaces;
using Application.IServices;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(option =>
            {
                option.AddPolicy("CORS", options =>
                {
                    options
                    .WithOrigins("Http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddDbContext<DocShareContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DocShare"));
            });
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(policyName: "fixedwindow", configureOptions =>
                {
                    configureOptions.QueueLimit = 0;
                    configureOptions.PermitLimit = 1000;
                    configureOptions.Window = TimeSpan.FromSeconds(100);
                    configureOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
                options.AddPolicy("per_ip", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                            ? httpContext.User.Identity.Name!
                            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 500,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0
                        }));
                options.AddPolicy("ip_login", httpcontext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpcontext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0
                        }));
                options.AddPolicy("export_file_light", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                            ? httpContext.User.Identity.Name!
                            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 5,
                                Window = TimeSpan.FromSeconds(30),
                                QueueLimit = 0
                            }));


                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    var key = Encoding.UTF8.GetBytes(builder.Configuration["SecretKey"] ?? string.Empty);
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                        RequireSignedTokens = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["accessToken"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            builder.Services.AddScoped<IUsers,UsersRepo>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IGoogleAuthService,GoogleAuthService>();
            builder.Services.AddScoped<IDocuments,DocumentsRepo>();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<RabbitMQService>();
            builder.Services.AddHostedService<RabbitMQWorker>();
            builder.Services.AddScoped<NotificationHub>();
            var app = builder.Build();
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseRateLimiter();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CORS");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");

            app.Run();

        }
    }
}