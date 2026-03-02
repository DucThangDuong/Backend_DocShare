using Amazon.S3;
using API.Extensions;
using API.Services;
using Application.Interfaces;
using Application.IServices;
using FastEndpoints;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
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
            // Add Memory Cache
            builder.Services.AddMemoryCache();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddFastEndpoints();
            builder.Services.AddRateLimiter(options =>
            {
                options.AddPolicy("auth_strict", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0
                        }));

                options.AddPolicy("token_refresh", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromSeconds(60),
                            QueueLimit = 0
                        }));

                options.AddPolicy("read_standard", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                            ? httpContext.User.Identity.Name!
                            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 30,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0
                        }));

                options.AddPolicy("read_public", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0
                        }));

                options.AddPolicy("write_standard", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                            ? httpContext.User.Identity.Name!
                            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0
                        }));

                options.AddPolicy("write_heavy", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.IsAuthenticated == true
                            ? httpContext.User.Identity.Name!
                            : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromSeconds(60),
                            QueueLimit = 0
                        }));

                options.AddPolicy("delete_action", httpContext =>
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
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            var storageConfig = builder.Configuration.GetSection("Storage");
            //Repositories
            builder.Services.AddDbContext<DocShareContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DocShare"));
            });
            builder.Services.AddScoped<IUsers, UsersRepo>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDocuments, DocumentsRepo>();
            builder.Services.AddScoped<ITags, TagRepo>();
            builder.Services.AddScoped<IUserActivity, UserActivity>();
            builder.Services.AddScoped<IUniversitites, UniversitiesRepo>();

            // Services
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            builder.Services.AddScoped<RabbitMQService>();

            // Document Feature Handlers
            builder.Services.AddScoped<Features.Documents.Commands.ClearDocumentFileHandler>();
            builder.Services.AddScoped<Features.Documents.Commands.CreateDocumentHandler>();
            builder.Services.AddScoped<Features.Documents.Commands.MoveToTrashHandler>();
            builder.Services.AddScoped<Features.Documents.Commands.ScanDocumentHandler>();
            builder.Services.AddScoped<Features.Documents.Commands.UpdateDocumentHandler>();
            builder.Services.AddScoped<Features.Documents.Queries.GetDocsOfUserHandler>();
            builder.Services.AddScoped<Features.Documents.Queries.GetDocumentDetailHandler>();
            builder.Services.AddScoped<Features.Documents.Queries.GetDocumentEditDetailHandler>();
            builder.Services.AddScoped<Features.Documents.Queries.GetUserDocStatsHandler>();
            builder.Services.AddScoped<Features.Tags.Queries.GetDocumentsOfTagHandler>();
            builder.Services.AddScoped<Features.Tags.Queries.GetTagHandler>();

            // Universities Feature Handlers
            builder.Services.AddScoped<Features.Universities.Queries.GetAllUniversitiesHandler>();
            builder.Services.AddScoped<Features.Universities.Queries.GetSectionsHandler>();
            builder.Services.AddScoped<Features.Universities.Queries.GetDocumentsOfSectionHandler>();
            builder.Services.AddScoped<Features.Universities.Queries.GetPopularDocumentsHandler>();
            builder.Services.AddScoped<Features.Universities.Queries.GetRecentDocumentsHandler>();
            builder.Services.AddScoped<Features.Universities.Commands.AddSectionHandler>();

            // User Feature Handlers
            builder.Services.AddScoped<Features.User.Queries.GetLikedDocsHandler>();
            builder.Services.AddScoped<Features.User.Queries.GetPrivateProfileHandler>();
            builder.Services.AddScoped<Features.User.Queries.GetPublicProfileHandler>();
            builder.Services.AddScoped<Features.User.Queries.GetPublicUserStatsHandler>();
            builder.Services.AddScoped<Features.User.Queries.GetSavedDocsHandler>();
            builder.Services.AddScoped<Features.User.Queries.GetUploadedDocsHandler>();
            builder.Services.AddScoped<Features.User.Queries.GetUserDocumentsHandler>();
            builder.Services.AddScoped<Features.User.Queries.GetUserStorageHandler>();
            builder.Services.AddScoped<Features.User.Commands.UpdateAvatarHandler>();
            builder.Services.AddScoped<Features.User.Commands.UpdatePasswordHandler>();
            builder.Services.AddScoped<Features.User.Commands.UpdateProfileHandler>();
            builder.Services.AddScoped<Features.User.Commands.UpdateUsernameHandler>();

            // UserActivity Feature Handlers
            builder.Services.AddScoped<Features.UserActivity.Commands.FollowUserHandler>();
            builder.Services.AddScoped<Features.UserActivity.Commands.UnfollowUserHandler>();
            builder.Services.AddScoped<Features.UserActivity.Commands.SaveDocumentHandler>();
            builder.Services.AddScoped<Features.UserActivity.Commands.VoteDocumentHandler>();
            builder.Services.AddScoped<Features.UserActivity.Queries.GetSavedDocumentsHandler>();
            // AWS S3
            builder.Services.AddDefaultAWSOptions(new Amazon.Extensions.NETCore.Setup.AWSOptions
            {
                Credentials = new Amazon.Runtime.BasicAWSCredentials(
                    storageConfig["AccessKey"],
                    storageConfig["SecretKey"]),
                Region = Amazon.RegionEndpoint.USEast1
            });

            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = storageConfig["ServiceUrl"],
                    ForcePathStyle = true
                };
                return new AmazonS3Client(
                    storageConfig["AccessKey"],
                    storageConfig["SecretKey"], config);
            });

            builder.Services.AddSingleton<IStorageService, S3StorageService>();

            // SignalR
            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            builder.Services.AddSingleton<ISignalRService, SignalRService>();

            // Background Services
            builder.Services.AddHostedService<RabbitMQWorker>();

            var app = builder.Build();
            StringHelpers.Initialize(builder.Configuration);
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CORS");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.UseFastEndpoints();
            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");
            app.Run();

        }
    }
}