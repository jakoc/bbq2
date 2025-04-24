using System.Text;
using BobsBBQApi.BLL;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL;
using BobsBBQApi.DAL.Repositories;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;
using BobsBBQApi.Helpers.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BobsBBQApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        builder.Services.AddDbContext<BobsBBQContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"));
        });

        builder.Services.AddScoped<IReservationLogic, ReservationLogic>();
        builder.Services.AddScoped<IUserLogic, UserLogic>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
        builder.Services.AddScoped<IPasswordEncrypter, PasswordEncrypter>();
        builder.Services.AddScoped<IJwtToken, JwtToken>();
        
        builder.Services.AddSingleton<Email>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var smtpServer = configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(configuration["Email:SmtpPort"]);
            var smtpUser = configuration["Email:SmtpUser"];
            var smtpPass = configuration["Email:SmtpPass"];
            return new Email(smtpServer, smtpPort, smtpUser, smtpPass);
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
        app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();
        app.MapControllers();
        app.Run();
    }
}