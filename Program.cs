using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RotaVerdeAPI.Data;
using RotaVerdeAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE CORS ---
// Unificamos as origens em uma única política para evitar conflitos de seleção
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "RotaVerdePolicy",
        policy =>
        {
            policy
                .AllowAnyOrigin() // Teste: permite qualquer origem
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

// --- 2. SERVIÇOS BASE ---
builder.Services.AddControllers();

// Configurar o banco de dados SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// --- 3. CONFIGURAR IDENTITY ---
builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Ajustes de senha opcionais para facilitar o teste inicial
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// --- 4. INICIALIZAÇÃO DE DADOS (SEED) ---
// Certifique-se que sua classe SeedData está correta
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Um erro ocorreu ao semear o banco de dados.");
    }
}

// --- 5. MIDDLEWARES (A ORDEM IMPORTA MUITO) ---

// O CORS deve ser o primeiro para responder às requisições "OPTIONS" do navegador
app.UseCors("RotaVerdePolicy");

// Se estiver no Docker/VPS com Nginx, o HTTPS Redirection às vezes é opcional,
// mas manteremos aqui por segurança.
app.UseHttpsRedirection();

app.UseAuthentication(); // Quem é o usuário?
app.UseAuthorization(); // O que ele pode fazer?

app.MapControllers();

app.Run();
