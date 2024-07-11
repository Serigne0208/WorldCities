using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WorldCities.Server.Data;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

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

// Add ApplicationDbContext and SQL Server support
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        )
);

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

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
