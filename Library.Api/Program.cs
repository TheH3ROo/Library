using Library.Application.Abstractions;
using Library.Application.Services;
using Library.Infrastructure.Data;
using Library.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseInMemoryDatabase("LibraryDb"));

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

builder.Services.AddScoped<ILoanService, LoanService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
