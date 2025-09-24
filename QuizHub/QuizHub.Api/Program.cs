using QuizHub.Data.Context;
using Microsoft.EntityFrameworkCore;
using QuizHub.Services.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuizHub.Services.Implementations;
using QuizHub.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddHttpContextAccessor();

// JWT Authentication
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "images")
    ),
    RequestPath = "/images"
});

app.UseHttpsRedirection();

app.UseCors();

// **Enable Authentication and Authorization**
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
