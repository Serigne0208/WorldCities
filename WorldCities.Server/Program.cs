using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WorldCities.Server.Data;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Microsoft.AspNetCore.Identity;
using WorldCities.Server.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WorldCities.Server.Data.GraphQL;

var builder = WebApplication.CreateBuilder(args);

// Adds Serilog support
builder.Host.UseSerilog((ctx, lc) => lc
 .ReadFrom.Configuration(ctx.Configuration)
 .WriteTo.MSSqlServer(connectionString:
 ctx.Configuration.GetConnectionString("DefaultConnection"),
 restrictedToMinimumLevel: LogEventLevel.Information,
 sinkOptions: new MSSqlServerSinkOptions
 {
     TableName = "LogEvents",
     AutoCreateSqlTable = true
 }
 )
 .WriteTo.Console()
 );

// Add services to the container.

builder.Services.AddControllers()
     .AddJsonOptions(options =>
     {
         //some indentation so that we’ll be able to understand more of those outputs.
         options.JsonSerializerOptions.WriteIndented = true;
         //If we wanted to switch from camelCase (default) to PascalCase,
        // options.JsonSerializerOptions.PropertyNamingPolicy = null;
     })
     ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    options.AddPolicy(name: "AngularPolicy",
        cfg => {
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
            cfg.WithOrigins(builder.Configuration["AllowedCORS"]!);
        }));


// Add ApplicationDbContext and SQL Server support
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        )
);


// Add ASP.NET Core Identity support
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
 .AddApiEndpoints()//Activating the Identity API endpoints
 .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<JwtHandler>();

//add GraphQL
builder.Services.AddGraphQLServer()
 .AddAuthorization()
 .AddQueryType<Query>()
 .AddMutationType<Mutation>()
 .AddFiltering()
 .AddSorting();
// Add Authentication services & middlewares
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RequireExpirationTime = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecurityKey"]!))
    };
}).AddBearerToken(IdentityConstants.BearerScheme);


var app = builder.Build();
//we need to add the UseSerilogRequestLogging middleware
app.UseSerilogRequestLogging();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("AngularPolicy");


app.MapIdentityApi<IdentityUser>();
app.MapControllers();

app.MapGraphQL("/api/graphql");

app.MapMethods("/api/heartbeat", new[] { "HEAD" },
 () => Results.Ok());

app.MapFallbackToFile("/index.html");

app.Run();
