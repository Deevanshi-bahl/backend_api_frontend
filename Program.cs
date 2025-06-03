using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyApiProject API", Version = "v1" });
});

var app = builder.Build();

// ---------- Middleware ----------
// REQUIRED for minimal APIs + Swagger

app.UseRouting();

app.UseDefaultFiles();      // This will serve wwwroot/index.html
app.UseStaticFiles();      // Enable static files like HTML, CSS, JS


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyApiProject API V1");
    c.RoutePrefix = "swagger"; // Swagger available at "/"
});

app.MapControllers(); // For attribute-based controllers (if added later)

// ---------- Minimal APIs with Swagger tags ----------

// GET: All credentials with profile
app.MapGet("/api/credentials", async (AppDbContext db) =>
    await db.Credentials.Include(c => c.Profile).ToListAsync())
    .WithName("GetAllCredentials")
    .WithTags("Credentials")
    .WithOpenApi(op => new(op)
    {
        Summary = "Get all credentials",
        Description = "Returns a list of all credentials along with their associated profile data."
    });

// GET: Credential by ID
app.MapGet("/api/credentials/{id:int}", async (int id, AppDbContext db) =>
{
    var credential = await db.Credentials.Include(c => c.Profile)
                                         .FirstOrDefaultAsync(c => c.CredentialID == id);
    return credential is not null ? Results.Ok(credential) : Results.NotFound();
})
.WithName("GetCredentialById")
.WithTags("Credentials")
.WithOpenApi(op => new(op)
{
    Summary = "Get a credential by ID",
    Description = "Returns a single credential and its profile, if it exists."
});

// ---------- Authentication Endpoints ----------

// POST: Register (hash password)
app.MapPost("/api/register", async (AppDbContext db, Credential credential) =>
{
    if (await db.Credentials.AnyAsync(c => c.Email == credential.Email))
        return Results.Conflict("Email already registered.");

    credential.PasswordHash = BCrypt.Net.BCrypt.HashPassword(credential.PasswordHash);

    db.Credentials.Add(credential);
    await db.SaveChangesAsync();

    return Results.Ok("Registration successful.");
})
.WithName("Register")
.WithTags("Authentication")
.WithOpenApi(op => new(op)
{
    Summary = "Register a new user",
    Description = "Creates a new credential with a securely hashed password."
});

// POST: Login (verify password)
app.MapPost("/api/login", async (AppDbContext db, Credential login) =>
{
    var user = await db.Credentials.FirstOrDefaultAsync(c => c.Email == login.Email);

    if (user == null || !BCrypt.Net.BCrypt.Verify(login.PasswordHash, user.PasswordHash))
        return Results.Unauthorized();

    return Results.Ok("Login successful.");
})
.WithName("Login")
.WithTags("Authentication")
.WithOpenApi(op => new(op)
{
    Summary = "Login a user",
    Description = "Validates credentials using secure password verification."
});

app.Run();

// ---------- Models & DbContext ----------

public class Credential
{
    public int CredentialID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    [JsonIgnore]
    public Profile? Profile { get; set; }
}

public class Profile
{
    public int ProfileID { get; set; }
    public int CredentialID { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }

    [JsonIgnore]
    public Credential? Credential { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<Profile> Profiles => Set<Profile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credential>()
            .HasIndex(c => c.Email)
            .IsUnique();

        modelBuilder.Entity<Credential>()
            .HasOne(c => c.Profile)
            .WithOne(p => p.Credential)
            .HasForeignKey<Profile>(p => p.CredentialID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
