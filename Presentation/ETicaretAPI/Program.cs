using Application;
using Application.Validators.Products;
using FluentValidation.AspNetCore;
using Infrastructure.Filters;
using Persistence;
using Infrastructure;
using Infrastructure.Services.Storage.Local;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options=>options.AddDefaultPolicy(policy=>policy.AllowAnyHeader()
    .AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200"))
);
builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

builder.Services.AddStorage<LocalStorage>();
//builder.Services.AddStorage();

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
     .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<CreateProductValidator>())
     .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);


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
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
