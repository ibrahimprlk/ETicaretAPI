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
using ETicaretAPI.Configurations.ColumnWriters;
using Microsoft.AspNetCore.HttpLogging;
using NpgsqlTypes;
using Serilog.Sinks.PostgreSQL;
using Serilog;
using Serilog.Core;
using System.Security.Claims;
using Serilog.Context;


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
             ValidateLifetime = true, //Olu?turulan token de?erinin s�resini kontrol edecek olan do?rulamad?r.
             ValidateIssuerSigningKey = true, //�retilecek token de?erinin uygulamam?za ait bir de?er oldu?unu ifade eden suciry key verisinin do?rulanmas?d?r.

             ValidAudience = builder.Configuration["Token:Audience"],
             ValidIssuer = builder.Configuration["Token:Issuer"],
             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
             LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => expires != null ? expires > DateTime.UtcNow : false,
             NameClaimType = ClaimTypes.Name //JWT �zerinde Name claimne kar??l?k gelen de?eri User.Identity.Name propertysinden elde edebiliriz.
         };
     });

builder.Services.AddStorage<LocalStorage>();
//builder.Services.AddStorage();


Logger log = new LoggerConfiguration()
     .WriteTo.Console()
     .WriteTo.File("logs/log.txt")
     .WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("PostgreSQL"), "logs",
         needAutoCreateTable: true,
         columnOptions: new Dictionary<string, ColumnWriterBase>
         {
             {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text)},
             {"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text)},
             {"level", new LevelColumnWriter(true , NpgsqlDbType.Varchar)},
             {"time_stamp", new TimestampColumnWriter(NpgsqlDbType.Timestamp)},
             {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text)},
             {"log_event", new LogEventSerializedColumnWriter(NpgsqlDbType.Json)},
             {"user_name", new UsernameColumnWriter()}
         })
     .WriteTo.Seq(builder.Configuration["Seq:ServerURL"])
     .Enrich.FromLogContext()
     .MinimumLevel.Information()
     .CreateLogger();

builder.Host.UseSerilog(log);

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    logging.RequestHeaders.Add("sec-ch-ua");
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
});





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


app.UseSerilogRequestLogging();

app.UseHttpLogging();
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    var username = context.User?.Identity?.IsAuthenticated != null || true ? context.User.Identity.Name : null;
    LogContext.PushProperty("user_name", username);
    await next();
});


app.MapControllers();

app.Run();
