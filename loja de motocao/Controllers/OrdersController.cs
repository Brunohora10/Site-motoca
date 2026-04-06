using EssenzStore.Models.ViewModels;
using EssenzStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EssenzStore.Models;
using EssenzStore.Data;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Controllers;

[Authorize, Route("minha-conta/pedidos")]
public class OrdersController : Controller
{
    private readonly IOrderService _orders;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IOrderService orders, UserManager<ApplicationUser> userManager)
    { _orders = orders; _userManager = userManager; }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var list = await _orders.GetUserOrdersAsync(user.Id);
        return View(new OrderListViewModel { Orders = list });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        var order = await _orders.GetOrderByIdAsync(id);
        if (order == null || order.UserId != user.Id) return NotFound();
        return View(new OrderDetailViewModel
        {
            Order = order,
            Shipment = order.Shipments.FirstOrDefault(),
            TrackingEvents = order.Shipments.FirstOrDefault()?.TrackingEvents.OrderByDescending(e => e.EventDate).ToList() ?? new(),
            Payment = order.Payments.FirstOrDefault()
        });
    }
}

[Route("rastrear")]
public class TrackingController : Controller
{
    private readonly ApplicationDbContext _db;
    public TrackingController(ApplicationDbContext db) => _db = db;

    [HttpGet("")]
    public IActionResult Index(string? codigo) => View(new TrackingViewModel { CodigoRastreio = codigo });

    [HttpPost("")]
    public async Task<IActionResult> Index(TrackingViewModel vm)
    {
        if (!string.IsNullOrWhiteSpace(vm.CodigoRastreio))
        {
            var shipment = await _db.Shipments
                .Include(s => s.TrackingEvents)
                .Include(s => s.Order)
                .FirstOrDefaultAsync(s => s.CodigoRastreio == vm.CodigoRastreio || s.Order!.NumeroPedido == vm.CodigoRastreio);
            if (shipment != null)
            {
                vm.Shipment = shipment;
                vm.Order = shipment.Order;
                vm.Events = shipment.TrackingEvents.OrderByDescending(e => e.EventDate).ToList();
            }
            else ModelState.AddModelError("", "Código não encontrado.");
        }
        return View(vm);
    }
}

[Route("paginas")]
public class PagesController : Controller
{
    private readonly ApplicationDbContext _db;
    public PagesController(ApplicationDbContext db) => _db = db;

    [HttpGet("{slug}")]
    public async Task<IActionResult> Show(string slug)
    {
        var page = await _db.StorePages.FirstOrDefaultAsync(p => p.Slug == slug && p.Publicado);
        if (page == null) return NotFound();
        return View(page);
    }
}
