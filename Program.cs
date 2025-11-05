using maria.Model;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("conn")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        // Check if DB exists and if there are any pending migrations
        if (dbContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying pending migrations...");
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors(options =>
            options.WithOrigins("*").
            AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseAuthentication();

app.UseDefaultFiles();
//app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

app.Run();
