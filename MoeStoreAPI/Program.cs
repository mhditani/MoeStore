using Microsoft.EntityFrameworkCore;
using MoeStore.Entities.DB;
using MoeStore.Services.Mapper;
using MoeStore.Services.Repository;
using MoeStore.Services.Repository.IRepository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MoeStoreConnection"));
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MoeStoreMapper));

// impelent interface
builder.Services.AddScoped<IContactRepo, ContactRepo>();
builder.Services.AddScoped<ISubjectRepo, SubjectRepo>();


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

app.Run();
