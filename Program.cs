using ControleFinanceiroApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
// NOVO USING: Necessário para o método EnsureDatabaseCreated
using Microsoft.Extensions.Hosting; 

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. CONFIGURAÇÃO DE SERVIÇOS
// ==========================================================

builder.Services.AddRazorPages();

// AJUSTE CRÍTICO PARA O AZURE/SQLITE: Define o caminho como 'HOME/app.db'
var appRoot = Environment.GetEnvironmentVariable("HOME") ?? "./"; 
var dbPath = Path.Combine(appRoot, "app.db"); 

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}")); // Usa o caminho dinâmico para o Azure
	
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// LINHA DE CHAMADA CORRIGIDA: Deve vir imediatamente após app.Build();
EnsureDatabaseCreated(app); 

// ==========================================================
// 2. PIPELINE DE REQUISIÇÃO
// ==========================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

// ==========================================================
// 3. FUNÇÃO AUXILIAR DE CRIAÇÃO DO DB (Deve ficar no final)
// ==========================================================

// FUNÇÃO PARA GARANTIR QUE O ARQUIVO app.db E AS TABELAS EXISTAM NO SERVIDOR
void EnsureDatabaseCreated(WebApplication app)
{
    // O IHost é a própria instância do WebApplication quando usamos o Minimal API.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            // Executa todas as migrações pendentes, criando o DB se não existir
            context.Database.Migrate(); 
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB on startup.");
            // Re-throw para forçar o Azure a registrar a falha
            throw; 
        }
    }
}