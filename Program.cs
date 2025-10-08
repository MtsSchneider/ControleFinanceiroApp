using ControleFinanceiroApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Necessário para o ILoggerFactory
using System.IO;
using Microsoft.AspNetCore.Builder;
using ControleFinanceiroApp.ModelBinders; // Necessário para WebApplication

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. CONFIGURAÇÃO DE SERVIÇOS
// ==========================================================

builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        // NOVO: Adiciona o provedor customizado. Ele será instanciado no pipeline.
        // O construtor do provedor será ajustado para injetar ILoggerFactory automaticamente.
        options.ModelBinderProviders.Insert(0, new EmptyToNullDecimalModelBinderProvider());
    });


// CONFIGURAÇÃO DO DB (Caminho dinâmico para Azure)
var appRoot = Environment.GetEnvironmentVariable("HOME") ?? "./";
var dbPath = Path.Combine(appRoot, "app.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// CONFIGURAÇÃO DE AUTENTICAÇÃO
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
  {
      options.LoginPath = "/Account/Login";
      options.ExpireTimeSpan = TimeSpan.FromHours(24);
      options.SlidingExpiration = true;
  });

// NOVO: Registra o Model Binder Provider como um serviço para permitir injeção
builder.Services.AddSingleton<EmptyToNullDecimalModelBinderProvider>();


var app = builder.Build();

// GARANTE A CRIAÇÃO DO DB NO SERVIDOR
EnsureDatabaseCreated(app);

// ==========================================================
// 2. PIPELINE DE REQUISIÇÃO
// ==========================================================

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


// FUNÇÃO AUXILIAR DE CRIAÇÃO DO DB
void EnsureDatabaseCreated(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB on startup.");
            throw;
        }
    }
}