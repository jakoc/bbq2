using System.Text;
using BobsBBQApi.BLL;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL;
using BobsBBQApi.DAL.Repositories;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;
using BobsBBQApi.Helpers.Interfaces;
using FeatureHubSDK;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace BobsBBQApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://0.0.0.0:8080");
        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddDbContext<BobsBBQContext>(options =>
        {
            options.UseMySql(
                builder.Configuration.GetConnectionString("DBConnection"),
                new MySqlServerVersion(new Version(10, 8, 2)) // Brug den MariaDB version du kører på staging
            );
        });
        
        //helpers
        builder.Services.AddScoped<IPasswordEncrypter, PasswordEncrypter>();
        builder.Services.AddScoped<IJwtToken, JwtToken>();
        
        builder.Services.AddSingleton<IEmail, Email>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            
            
            var smtpServer = configuration["Email:SmtpServer"];
            var smtpPortStr = configuration["Email:SmtpPort"];
            var smtpUser = configuration["Email:SmtpUser"];
            var smtpPass = configuration["Email:SmtpPass"];

            var smtpPort = !string.IsNullOrWhiteSpace(smtpPortStr) ? int.Parse(smtpPortStr) : 587;

            return new Email(smtpServer, smtpPort, smtpUser, smtpPass);
        });
        
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("BobsBBQApi"))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            }); 
        //BLL
        builder.Services.AddScoped<IReservationLogic, ReservationLogic>();
        builder.Services.AddScoped<IUserLogic, UserLogic>();
        builder.Services.AddScoped<ITableLogic, TableLogic>();
        //DAL
        builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ITableRepository, TableRepository>();
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        var key = Encoding.ASCII.GetBytes("your_new_32_byte_or_longer_key_here_12345");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            };
            options.Events = new JwtBearerEvents
            {
            OnMessageReceived = context =>
            {
                if (context.HttpContext.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    context.NoResult();  // Ignore token validation for OPTIONS requests
                }
                return Task.CompletedTask;
            }
        };
        });
        
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("UserOnly", policy =>
                policy.RequireClaim("EntityType", "Customer"));
            options.AddPolicy("TechnicianOnly", policy =>
                policy.RequireClaim("EntityType", "Admin"));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.UseRouting(); 
        app.UseCors("AllowAll"); 
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseDefaultFiles(); 
        app.UseStaticFiles();
        app.MapControllers();
        app.Run();
    }
}