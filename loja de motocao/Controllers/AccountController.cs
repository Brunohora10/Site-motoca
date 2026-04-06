using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using EssenzStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EssenzStore.Data;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _email;
    private readonly IOrderService _orders;
    private readonly ApplicationDbContext _db;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IEmailService email, IOrderService orders, ApplicationDbContext db)
    {
        _userManager = userManager; _signInManager = signInManager;
        _email = email; _orders = orders; _db = db;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                return Redirect("/admin");

            return RedirectToAction("Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && !user.Ativo)
        {
            ModelState.AddModelError("", "Sua conta está inativa. Entre em contato com o suporte.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Senha, model.LembrarMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin") || roles.Contains("Manager"))
                    return Redirect("/admin");
            }

            return Redirect("/minha-conta");
        }

        if (result.IsLockedOut) ModelState.AddModelError("", "Conta bloqueada temporariamente. Tente novamente em alguns minutos.");
        else ModelState.AddModelError("", "E-mail ou senha incorretos.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser
        {
            UserName = model.Email, Email = model.Email,
            Nome = model.Nome, Sobrenome = model.Sobrenome,
            PhoneNumber = model.Telefone, Cpf = model.Cpf,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, model.Senha);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: false);
            await _email.SendWelcomeAsync(user.Email!, user.Nome);
            return RedirectToAction("Dashboard");
        }
        foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme)!;
            await _email.SendForgotPasswordAsync(model.Email, user.Nome, link);
        }
        TempData["Success"] = "Se o e-mail existir, você receberá as instruções em breve.";
        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email) =>
        View(new ResetPasswordViewModel { Token = token, Email = email });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null)
        {
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NovaSenha);
            if (result.Succeeded) { TempData["Success"] = "Senha redefinida com sucesso!"; return RedirectToAction("Login"); }
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        }
        return View(model);
    }

    [Authorize, HttpGet("/minha-conta")]
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");
        var orders = await _orders.GetUserOrdersAsync(user.Id);
        var favoritos = await _db.WishlistItems.CountAsync(w => w.UserId == user.Id);
        var enderecos = await _db.Addresses.Where(a => a.UserId == user.Id).ToListAsync();
        return View(new AccountDashboardViewModel
        {
            User = user,
            RecentOrders = orders.Take(5).ToList(),
            TotalPedidos = orders.Count,
            TotalFavoritos = favoritos,
            Addresses = enderecos
        });
    }

    // ── Perfil ───────────────────────────────────────────────────────────────
    [Authorize, HttpGet("/minha-conta/perfil")]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");
        var addresses = await _db.Addresses.Where(a => a.UserId == user.Id).OrderByDescending(a => a.Principal).ToListAsync();
        return View(new ProfileViewModel
        {
            Nome = user.Nome, Sobrenome = user.Sobrenome,
            Email = user.Email!, Telefone = user.PhoneNumber, Cpf = user.Cpf,
            Addresses = addresses
        });
    }

    [Authorize, HttpPost("/minha-conta/perfil"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");
        user.Nome = model.Nome; user.Sobrenome = model.Sobrenome;
        user.PhoneNumber = model.Telefone; user.Cpf = model.Cpf;
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded) TempData["Success"] = "Perfil atualizado!";
        else foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
        return View(model);
    }

    [Authorize, HttpPost("/minha-conta/senha"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");
        if (!ModelState.IsValid) { TempData["Error"] = "Dados inválidos."; return RedirectToAction("Profile"); }
        var result = await _userManager.ChangePasswordAsync(user, model.SenhaAtual, model.NovaSenha);
        if (result.Succeeded) TempData["Success"] = "Senha alterada com sucesso!";
        else TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
        return RedirectToAction("Profile");
    }

    // ── Endereços ────────────────────────────────────────────────────────────
    [Authorize, HttpPost("/minha-conta/enderecos/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAddress(Address model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        Address? address;
        if (model.Id > 0)
        {
            address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == model.Id && a.UserId == user.Id);
            if (address == null) return Forbid();
        }
        else
        {
            address = new Address { UserId = user.Id };
            _db.Addresses.Add(address);
        }

        address.Apelido = model.Apelido;
        address.Cep = model.Cep;
        address.Rua = model.Rua;
        address.Numero = model.Numero;
        address.Complemento = model.Complemento;
        address.Bairro = model.Bairro;
        address.Cidade = model.Cidade;
        address.Estado = model.Estado;
        address.Referencia = model.Referencia;
        address.Principal = model.Principal;
        address.UpdatedAt = DateTime.UtcNow;

        if (model.Principal)
        {
            var outros = await _db.Addresses.Where(a => a.UserId == user.Id && a.Id != address.Id).ToListAsync();
            outros.ForEach(a => a.Principal = false);
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Endereço salvo com sucesso!";
        return RedirectToAction("Profile", new { tab = "enderecos" });
    }

    [Authorize, HttpPost("/minha-conta/enderecos/{id:int}/excluir"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);
        if (address != null) { _db.Addresses.Remove(address); await _db.SaveChangesAsync(); }

        TempData["Success"] = "Endereço removido.";
        return RedirectToAction("Profile", new { tab = "enderecos" });
    }
}
