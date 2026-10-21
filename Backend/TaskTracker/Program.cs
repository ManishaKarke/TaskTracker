using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.DataAccess.Context;
using TaskTracker.DataAccess.Repository.Implementation;
using TaskTracker.DataAccess.Repository.Interfaces;
using TaskTracker.Middleware;
using TaskTracker.Services.Implementations;
using TaskTracker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IProjectRepository,ProjectRepository>();
builder.Services.AddScoped<ITaskRepository,TaskRepository>();

builder.Services.AddScoped<IProjectServices, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:4200/").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularDevelopment");

app.UseAuthorization();

app.MapControllers();

app.Run();
