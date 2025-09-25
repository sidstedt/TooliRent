using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using TooliRent.Domain.Entities;
using TooliRent.Infrastructure.Persistence;
using TooliRent.WebApi.Auth;
using TooliRent.Application.Interfaces;
using TooliRent.Application.Services;
using TooliRent.Domain.Interfaces;
using TooliRent.Infrastructure.Repositories;
using TooliRent.Application.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;
using TooliRent.Application.DTOs;
using TooliRent.Application.Validation;

namespace TooliRent.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<TooliRentDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(opt =>
            {
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Use your token"
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Microsoft Identity setup
            builder.Services.AddIdentityCore<ApplicationUser>(opt =>
                {
                    opt.User.RequireUniqueEmail = true;
                    opt.Password.RequiredLength = 6;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<TooliRentDbContext>()
                .AddDefaultTokenProviders();

            // JWT setup
            var jwt = builder.Configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidAudience = jwt["Audience"],
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });
            builder.Services.AddAuthorization();

            // AutoMapper
            builder.Services.AddAutoMapper(cfg => { }, typeof(ToolProfile));
            builder.Services.AddAutoMapper(cfg => { }, typeof(ToolCategoryProfile));
            builder.Services.AddAutoMapper(cfg => { }, typeof(BookingProfile));

            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            // DI: repositories and services
            builder.Services.AddScoped<IToolRepository, ToolRepository>();
            builder.Services.AddScoped<IToolService, ToolService>();

            builder.Services.AddScoped<IToolCategoryRepository, ToolCategoryRepository>();
            builder.Services.AddScoped<IToolCategoryService, ToolCategoryService>();

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();
            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();

            // Fluent validator
            builder.Services.AddFluentValidationAutoValidation(o => o.DisableDataAnnotationsValidation = true);
            builder.Services.AddValidatorsFromAssembly(typeof(Application.Validation.BookingItemCreateDtoValidator).Assembly);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
