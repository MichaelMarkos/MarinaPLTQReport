using maria.Helpers;
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll" ,
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

var app = builder.Build();





using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 1. If history table is empty, baseline it
    if (!db.Database.GetAppliedMigrations().Any())
    {
        var allMigrations = db.Database.GetMigrations();
        var productVersion = typeof(DbContext).Assembly
            .GetName().Version?.ToString() ?? "8.0.0"; // fallback

        foreach (var migration in allMigrations)
        {
            db.Database.ExecuteSqlRaw(@"
                INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                VALUES ({0}, {1})",
                migration, productVersion);
        }
    }

    // 2. Apply any new migrations normally
    db.Database.Migrate();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseDefaultFiles();
//app.UseStaticFiles();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();

