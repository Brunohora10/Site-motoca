using EssenzStore.Data;
using EssenzStore.Models;
using EssenzStore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC com Razor Views
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

// Entity Framework + SQLite
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=essenzstore.db"));

// ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireUppercase = false;
    opts.Lockout.MaxFailedAccessAttempts = 5;
    opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie de autenticação
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.LoginPath = "/login";
    opts.LogoutPath = "/logout";
    opts.AccessDeniedPath = "/acesso-negado";
    opts.ExpireTimeSpan = TimeSpan.FromDays(30);
    opts.SlidingExpiration = true;
    opts.Cookie.HttpOnly = true;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Session (carrinho de guest)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromHours(2);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.Cookie.SameSite = SameSiteMode.Lax;
});

// Serviços de negócio
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("MercadoPago", client =>
{
    client.BaseAddress = new Uri("https://api.mercadopago.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed do banco de dados
await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Rotas
app.MapHealthChecks("/health");
app.MapControllerRoute("admin", "admin/{action=Dashboard}/{id?}", new { controller = "Admin" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Atalhos de URL amigáveis
app.MapGet("/login", ctx => { ctx.Response.Redirect("/Account/Login"); return Task.CompletedTask; });
app.MapGet("/cadastro", ctx => { ctx.Response.Redirect("/Account/Register"); return Task.CompletedTask; });
app.MapGet("/logout", async ctx =>
{
    await ctx.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>().SignOutAsync();
    ctx.Response.Redirect("/");
});
app.MapGet("/produtos", ctx => { ctx.Response.Redirect("/Products/Index"); return Task.CompletedTask; });
app.MapGet("/carrinho", ctx => { ctx.Response.Redirect("/Cart/Index"); return Task.CompletedTask; });
app.MapGet("/checkout", ctx => { ctx.Response.Redirect("/Checkout/Index"); return Task.CompletedTask; });
app.MapGet("/faq", ctx => { ctx.Response.Redirect("/paginas/faq-perguntas-frequentes"); return Task.CompletedTask; });

app.Run();
