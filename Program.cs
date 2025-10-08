using ControleFinanceiroApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Necess�rio para o ILoggerFactory
using System.IO;
using Microsoft.AspNetCore.Builder;
using ControleFinanceiroApp.ModelBinders; // Necess�rio para WebApplication

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. CONFIGURA��O DE SERVI�OS
// ==========================================================

builder.Services.AddRazorPages()
    .AddMvcOptions(options =>
    {
        // NOVO: Adiciona o provedor customizado. Ele ser� instanciado no pipeline.
        // O construtor do provedor ser� ajustado para injetar ILoggerFactory automaticamente.
        options.ModelBinderProviders.Insert(0, new EmptyToNullDecimalModelBinderProvider());
    });


// CONFIGURA��O DO DB (Caminho din�mico para Azure)
var appRoot = Environment.GetEnvironmentVariable("HOME") ?? "./";
var dbPath = Path.Combine(appRoot, "app.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// CONFIGURA��O DE AUTENTICA��O
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
  {
      options.LoginPath = "/Account/Login";
      options.ExpireTimeSpan = TimeSpan.FromHours(24);
      options.SlidingExpiration = true;
  });

// NOVO: Registra o Model Binder Provider como um servi�o para permitir inje��o
builder.Services.AddSingleton<EmptyToNullDecimalModelBinderProvider>();


var app = builder.Build();

// GARANTE A CRIA��O DO DB NO SERVIDOR
EnsureDatabaseCreated(app);

// ==========================================================
// 2. PIPELINE DE REQUISI��O
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


// FUN��O AUXILIAR DE CRIA��O DO DB
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