using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MVP Back-End API",
        Version = "v1",
        Description = "Документація REST API навчального MVP-проєкту."
    });

	options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
    	Name = "Authorization",
    	Type = SecuritySchemeType.Http,
    	Scheme = "bearer",
    	BearerFormat = "JWT",
    	In = ParameterLocation.Header,
    	Description = "Введіть JWT-токен, отриманий з ендпоінта /auth/login."
	});

    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { 
            new OpenApiSecuritySchemeReference("Bearer", doc), 
            new List<string>() 
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MVP Back-End API v1");
    options.DocumentTitle = "MVP Back-End API";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "MVP Back-End із SQLite працює!");

app.MapGet("/recipes", async (AppDbContext db) => await db.Recipes.ToListAsync()).WithTags("Recipes");

app.MapGet("/recipes/{id}", async (int id, AppDbContext db) =>
    await db.Recipes.FindAsync(id) is Recipe recipe
        ? Results.Ok(recipe)
        : Results.NotFound())
    .WithTags("Recipes");

app.MapPost("/recipes", async (Recipe recipe, AppDbContext db) =>
{
    db.Recipes.Add(recipe);
    await db.SaveChangesAsync();
    return Results.Created($"/recipes/{recipe.Id}", recipe);
}).WithTags("Recipes");

app.MapPut("/recipes/{id}", async (int id, Recipe input, AppDbContext db) =>
{
    var recipe = await db.Recipes.FindAsync(id);
    if (recipe is null) return Results.NotFound();
    
    recipe.Name = input.Name;
    recipe.ItemResult = input.ItemResult;
    recipe.Count = input.Count;
	recipe.IsShapeless = input.IsShapeless;
    recipe.Ingredients = input.Ingredients;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("Recipes");

app.MapDelete("/recipes/{id}", async (int id, AppDbContext db) =>
{
    var recipe = await db.Recipes.FindAsync(id);
    if (recipe is null) return Results.NotFound();

    db.Recipes.Remove(recipe);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
}).WithTags("Recipes");

app.MapPost("/auth/register", async (RegisterDto dto, AppDbContext db) =>
{
    if (await db.Users.AnyAsync(u => u.Email == dto.Email))
        return Results.Conflict("Користувач з таким email вже існує.");

    var moderator = new User
    {
        Name = dto.Name,
        Email = dto.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        Role = "moderator"
    };

    db.Users.Add(moderator);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{moderator.Id}", new { moderator.Id, moderator.Name, moderator.Email, moderator.Role });
})
.WithTags("Auth");

app.MapPost("/auth/login", async (LoginDto dto, AppDbContext db, IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Results.Unauthorized();

    var token = CreateToken(user, config);
    return Results.Ok(new { access_token = token, token_type = "Bearer" });
})
.WithTags("Auth");

app.MapGet("/auth/me", (ClaimsPrincipal principal) =>
{
    return Results.Ok(new
    {
        Id = principal.FindFirstValue(ClaimTypes.NameIdentifier),
        Email = principal.FindFirstValue(ClaimTypes.Email),
        Role = principal.FindFirstValue(ClaimTypes.Role)
    });
})
.RequireAuthorization()
.WithTags("Auth");

static string CreateToken(User user, IConfiguration config)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.UseStaticFiles();

app.Run();

