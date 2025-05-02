using Application;
using Application.Validators.Products;
using FluentValidation.AspNetCore;
using Infrastructure.Filters;
using Persistence;
using Infrastructure;
using Infrastructure.Services.Storage.Local;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options=>options.AddDefaultPolicy(policy=>policy.AllowAnyHeader()
    .AllowAnyMethod().WithOrigins("http://localhost:4200", "https://localhost:4200"))
);
builder.Services.AddPersistenceServices();
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer("Admin", options =>
     {
         options.TokenValidationParameters = new()
         {
             ValidateAudience = true, //Olu?turulacak token de?erini kimlerin/hangi originlerin/sitelerin kullan?c? belirledi?imiz de?erdir. -> www.bilmemne.com
             ValidateIssuer = true, //Olu?turulacak token de?erini kimin da??tt?n? ifade edece?imiz aland?r. -> www.myapi.com
             ValidateLifetime = true, //Olu?turulan token de?erinin süresini kontrol edecek olan do?rulamad?r.
             ValidateIssuerSigningKey = true, //Üretilecek token de?erinin uygulamam?za ait bir de?er oldu?unu ifade eden suciry key verisinin do?rulanmas?d?r.

             ValidAudience = builder.Configuration["Token:Audience"],
             ValidIssuer = builder.Configuration["Token:Issuer"],
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"]))
         };
     });

builder.Services.AddStorage<LocalStorage>();
//builder.Services.AddStorage();

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
     .AddFluentValidation(configuration => configuration.RegisterValidatorsFromAssemblyContaining<CreateProductValidator>())
     .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);


builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });

    opt.AddSecurityRequirement(new() {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new() {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
