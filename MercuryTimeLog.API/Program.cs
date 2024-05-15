using MercuryTimeLog.API.Common;
using MercuryTimeLog.API.Data;
using MercuryTimeLog.Domain.Common;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configStorage = builder.Configuration
                .GetSection("AzureStorageConfig")
                .Get<AzureStorageConfig>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
.AddDbContext<AppDbContext>(opts =>
{
    opts
        .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Scan(scan => scan.FromAssemblies(AppDomain.CurrentDomain.Load("MercuryTimeLog.API"))
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddSingleton(configStorage!);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ExceptionHandler>();

app.Run();
