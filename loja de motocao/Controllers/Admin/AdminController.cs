using EssenzStore.Data;
using EssenzStore.Models;
using EssenzStore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Controllers.Admin;

[Authorize(Roles = "Admin,Manager"), Route("admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(ApplicationDbContext db, IWebHostEnvironment env,
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _env = env;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────
    [HttpGet(""), HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

        var pedidosPagosMes = await _db.Orders
            .Where(o => o.CreatedAt >= inicioMes && o.PaymentStatus == PaymentStatus.Approved)
            .CountAsync();
        var fatMes = await _db.Orders
            .Where(o => o.CreatedAt >= inicioMes && o.PaymentStatus == PaymentStatus.Approved)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var vm = new AdminDashboardViewModel
        {
            FaturamentoHoje = await _db.Orders
                .Where(o => o.CreatedAt >= hoje && o.PaymentStatus == PaymentStatus.Approved)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            FaturamentoMes = fatMes,
            TicketMedio = pedidosPagosMes > 0 ? fatMes / pedidosPagosMes : 0,
            PedidosPendentes = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
            PedidosPagos = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Paid),
            PedidosEnviados = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Shipped),
            TotalClientes = await _db.Users.CountAsync(),
            TotalProdutos = await _db.Products.CountAsync(p => p.Ativo),
            UltimosPedidos = await _db.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync()
        };

        vm.ProdutosMaisVendidos = (await _db.OrderItems
            .GroupBy(i => i.NomeProdutoSnapshot)
            .Select(g => new { Nome = g.Key, Vendas = g.Sum(i => i.Quantidade) })
            .OrderByDescending(x => x.Vendas)
            .Take(5)
            .ToListAsync())
            .Select(x => (x.Nome, x.Vendas))
            .ToList();

        vm.EstoqueBaixo = (await _db.ProductVariants
            .Where(v => v.Ativo && v.Estoque <= 3)
            .OrderBy(v => v.Estoque)
            .Take(10)
            .Select(v => new { Sku = v.Sku ?? "—", v.Estoque })
            .ToListAsync())
            .Select(x => (x.Sku, x.Estoque))
            .ToList();

        return View("~/Views/Admin/Dashboard/Index.cshtml", vm);
    }

    // ── Produtos ──────────────────────────────────────────────────────────────
    [HttpGet("produtos")]
    public async Task<IActionResult> ListarProdutos(string? q, int page = 1)
    {
        const int pageSize = 15;
        var query = _db.Products.Include(p => p.Brand).Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Nome.Contains(q) || (p.Brand != null && p.Brand.Nome.Contains(q)));
        var total = await query.CountAsync();
        var products = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Q = q;
        return View("~/Views/Admin/Products/Index.cshtml", products);
    }

    [HttpGet("produtos/novo")]
    public async Task<IActionResult> NovoProduto()
    {
        var vm = new AdminProductViewModel
        {
            Brands = await _db.Brands.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync(),
            Categories = await _db.Categories.Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync()
        };
        return View("~/Views/Admin/Products/Create.cshtml", vm);
    }

    [HttpPost("produtos/novo"), ValidateAntiForgeryToken]
    public async Task<IActionResult> NovoProduto(AdminProductViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Brands = await _db.Brands.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync();
            vm.Categories = await _db.Categories.Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync();
            return View("~/Views/Admin/Products/Create.cshtml", vm);
        }
        var product = new Product
        {
            Nome = vm.Nome,
            Slug = string.IsNullOrWhiteSpace(vm.Slug) ? GenerateSlug(vm.Nome) : vm.Slug,
            BrandId = vm.BrandId, CategoryId = vm.CategoryId,
            DescricaoCurta = vm.DescricaoCurta, DescricaoCompleta = vm.DescricaoCompleta,
            Composicao = vm.Composicao, Modelagem = vm.Modelagem,
            InstrucoesLavagem = vm.InstrucoesLavagem,
            Preco = vm.Preco, PrecoPromocional = vm.PrecoPromocional,
            Ativo = vm.Ativo, Destaque = vm.Destaque, Lancamento = vm.Lancamento,
            MaisVendido = vm.MaisVendido, ProdutoMomento = vm.ProdutoMomento,
            SeoTitle = vm.SeoTitle, SeoDescription = vm.SeoDescription
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Produto criado com sucesso!";
        return RedirectToAction("EditarProduto", new { id = product.Id });
    }

    [HttpGet("produtos/{id:int}/editar")]
    public async Task<IActionResult> EditarProduto(int id)
    {
        var p = await _db.Products
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        var vm = new AdminProductViewModel
        {
            Id = p.Id, Nome = p.Nome, Slug = p.Slug, BrandId = p.BrandId,
            CategoryId = p.CategoryId, DescricaoCurta = p.DescricaoCurta,
            DescricaoCompleta = p.DescricaoCompleta, Composicao = p.Composicao,
            Modelagem = p.Modelagem, InstrucoesLavagem = p.InstrucoesLavagem,
            Preco = p.Preco, PrecoPromocional = p.PrecoPromocional,
            Ativo = p.Ativo, Destaque = p.Destaque, Lancamento = p.Lancamento,
            MaisVendido = p.MaisVendido, ProdutoMomento = p.ProdutoMomento,
            SeoTitle = p.SeoTitle, SeoDescription = p.SeoDescription,
            Variants = p.Variants.OrderBy(v => v.Tamanho).ThenBy(v => v.Cor).ToList(),
            Images = p.Images.OrderBy(i => i.Ordem).ToList(),
            Brands = await _db.Brands.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync(),
            Categories = await _db.Categories.Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync()
        };
        return View("~/Views/Admin/Products/Edit.cshtml", vm);
    }

    [HttpPost("produtos/{id:int}/editar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarProduto(int id, AdminProductViewModel vm)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        if (!ModelState.IsValid)
        {
            vm.Id = id;
            vm.Variants = await _db.ProductVariants.Where(v => v.ProductId == id).ToListAsync();
            vm.Images = await _db.ProductImages.Where(i => i.ProductId == id).OrderBy(i => i.Ordem).ToListAsync();
            vm.Brands = await _db.Brands.Where(b => b.Ativo).OrderBy(b => b.Nome).ToListAsync();
            vm.Categories = await _db.Categories.Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync();
            return View("~/Views/Admin/Products/Edit.cshtml", vm);
        }
        p.Nome = vm.Nome;
        p.Slug = string.IsNullOrWhiteSpace(vm.Slug) ? GenerateSlug(vm.Nome) : vm.Slug;
        p.BrandId = vm.BrandId; p.CategoryId = vm.CategoryId;
        p.DescricaoCurta = vm.DescricaoCurta; p.DescricaoCompleta = vm.DescricaoCompleta;
        p.Composicao = vm.Composicao; p.Modelagem = vm.Modelagem;
        p.InstrucoesLavagem = vm.InstrucoesLavagem;
        p.Preco = vm.Preco; p.PrecoPromocional = vm.PrecoPromocional;
        p.Ativo = vm.Ativo; p.Destaque = vm.Destaque; p.Lancamento = vm.Lancamento;
        p.MaisVendido = vm.MaisVendido; p.ProdutoMomento = vm.ProdutoMomento;
        p.SeoTitle = vm.SeoTitle; p.SeoDescription = vm.SeoDescription;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Produto atualizado!";
        return RedirectToAction("EditarProduto", new { id });
    }

    [HttpPost("produtos/{id:int}/excluir"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirProduto(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p != null) { p.Ativo = false; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Produto desativado.";
        return RedirectToAction("ListarProdutos");
    }

    // ── Categorias ────────────────────────────────────────────────────────────
    [HttpGet("categorias")]
    public async Task<IActionResult> ListarCategorias()
    {
        var cats = await _db.Categories.Include(c => c.Parent).OrderBy(c => c.Ordem).ToListAsync();
        return View("~/Views/Admin/Categories/Index.cshtml", cats);
    }

    [HttpPost("categorias/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarCategoria(int Id, string Nome, string? Slug, int ParentId, int Ordem, bool Ativo = false)
    {
        var cat = Id > 0 ? await _db.Categories.FindAsync(Id) ?? new Category() : new Category();
        cat.Nome = Nome;
        cat.Slug = string.IsNullOrWhiteSpace(Slug) ? GenerateSlug(Nome) : Slug;
        cat.ParentId = ParentId > 0 ? ParentId : null;
        cat.Ordem = Ordem; cat.Ativo = Ativo;
        if (Id == 0) _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Categoria salva!";
        return RedirectToAction("ListarCategorias");
    }

    // ── Marcas ────────────────────────────────────────────────────────────────
    [HttpGet("marcas")]
    public async Task<IActionResult> ListarMarcas()
    {
        var brands = await _db.Brands.OrderBy(b => b.Ordem).ToListAsync();
        return View("~/Views/Admin/Brands/Index.cshtml", brands);
    }

    [HttpPost("marcas/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarMarca(int Id, string Nome, string? Slug, string? LogoUrl, int Ordem, bool Ativo = false, bool Destaque = false)
    {
        var brand = Id > 0 ? await _db.Brands.FindAsync(Id) ?? new Brand() : new Brand();
        brand.Nome = Nome;
        brand.Slug = string.IsNullOrWhiteSpace(Slug) ? GenerateSlug(Nome) : Slug;
        brand.LogoUrl = LogoUrl; brand.Ordem = Ordem; brand.Ativo = Ativo; brand.Destaque = Destaque;
        if (Id == 0) _db.Brands.Add(brand);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Marca salva!";
        return RedirectToAction("ListarMarcas");
    }

    // ── Banners ───────────────────────────────────────────────────────────────
    [HttpGet("banners")]
    public async Task<IActionResult> ListarBanners()
    {
        var banners = await _db.Banners.OrderBy(b => b.Ordem).ToListAsync();
        return View("~/Views/Admin/Banners/Index.cshtml", banners);
    }

    [HttpPost("banners/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarBanner(int Id, string Titulo, string? Subtitulo,
        string? ImagemDesktop, string? ImagemMobile, string? Link, string? TextoBotao,
        string Posicao, int Ordem, DateTime? DataInicio, DateTime? DataFim, bool Ativo = false)
    {
        var banner = Id > 0 ? await _db.Banners.FindAsync(Id) ?? new Banner() : new Banner();
        banner.Titulo = Titulo; banner.Subtitulo = Subtitulo;
        banner.ImagemDesktop = ImagemDesktop ?? ""; banner.ImagemMobile = ImagemMobile;
        banner.Link = Link; banner.TextoBotao = TextoBotao;
        banner.Posicao = Posicao; banner.Ordem = Ordem; banner.Ativo = Ativo;
        banner.DataInicio = DataInicio; banner.DataFim = DataFim;
        if (Id == 0) _db.Banners.Add(banner);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Banner salvo!";
        return RedirectToAction("ListarBanners");
    }

    // ── Cupons ────────────────────────────────────────────────────────────────
    [HttpGet("cupons")]
    public async Task<IActionResult> ListarCupons()
    {
        var coupons = await _db.Coupons.OrderByDescending(c => c.Id).ToListAsync();
        return View("~/Views/Admin/Coupons/Index.cshtml", coupons);
    }

    [HttpPost("cupons/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarCupom(AdminCouponViewModel vm)
    {
        var coupon = vm.Id > 0 ? await _db.Coupons.FindAsync(vm.Id) ?? new Coupon() : new Coupon();
        coupon.Codigo = vm.Codigo.ToUpperInvariant(); coupon.Tipo = vm.Tipo;
        coupon.Valor = vm.Valor; coupon.ValorMinimo = vm.ValorMinimo;
        coupon.UsoMaximo = vm.UsoMaximo; coupon.UsoPorCliente = vm.UsoPorCliente;
        coupon.DataInicio = vm.DataInicio; coupon.DataFim = vm.DataFim;
        coupon.Ativo = vm.Ativo; coupon.Descricao = vm.Descricao;
        if (vm.Id == 0) _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cupom salvo!";
        return RedirectToAction("ListarCupons");
    }

    // ── Pedidos ───────────────────────────────────────────────────────────────
    [HttpGet("pedidos")]
    public async Task<IActionResult> ListarPedidos(string? q, string? status, int page = 1)
    {
        const int pageSize = 20;
        var query = _db.Orders.Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(o => o.NumeroPedido.Contains(q) ||
                                     (o.EmailCliente ?? "").Contains(q) ||
                                     (o.NomeCliente ?? "").Contains(q));
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var s))
            query = query.Where(o => o.Status == s);
        var total = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return View("~/Views/Admin/Orders/Index.cshtml", new AdminOrderViewModel
        {
            Orders = orders, StatusFilter = status, SearchQuery = q,
            Page = page, TotalPages = (int)Math.Ceiling(total / (double)pageSize), TotalCount = total
        });
    }

    [HttpGet("devolucoes")]
    public async Task<IActionResult> ListarDevolucoes(string? q, int page = 1)
    {
        const int pageSize = 20;

        var query = _db.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == OrderStatus.Returned ||
                        o.Status == OrderStatus.Refunded ||
                        o.Status == OrderStatus.Cancelled);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(o => o.NumeroPedido.Contains(q) ||
                                     (o.EmailCliente ?? "").Contains(q) ||
                                     (o.NomeCliente ?? "").Contains(q));

        var total = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return View("~/Views/Admin/Orders/Returns.cshtml", new AdminOrderViewModel
        {
            Orders = orders,
            SearchQuery = q,
            Page = page,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            TotalCount = total
        });
    }

    [HttpGet("pedidos/{id:int}")]
    public async Task<IActionResult> DetalhePedido(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Include(o => o.Shipments).ThenInclude(s => s.TrackingEvents)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order == null ? NotFound() : View("~/Views/Admin/Orders/Detail.cshtml", order);
    }

    [HttpPost("pedidos/{id:int}/status"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarStatus(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order != null && Enum.TryParse<OrderStatus>(status, out var s))
        {
            order.Status = s;
            order.UpdatedAt = DateTime.UtcNow;

            if (s == OrderStatus.Paid)
                order.PaymentStatus = PaymentStatus.Approved;

            if (s == OrderStatus.Cancelled && order.PaymentStatus == PaymentStatus.Pending)
                order.PaymentStatus = PaymentStatus.Cancelled;

            if (s == OrderStatus.Refunded)
                order.PaymentStatus = PaymentStatus.Refunded;

            if (s == OrderStatus.Shipped)
                order.ShippingStatus = ShippingStatus.Posted;

            if (s == OrderStatus.Delivered)
                order.ShippingStatus = ShippingStatus.Delivered;

            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Status atualizado!";
        return RedirectToAction("DetalhePedido", new { id });
    }

    // ── Clientes ──────────────────────────────────────────────────────────────
    [HttpGet("clientes")]
    public async Task<IActionResult> ListarClientes(string? q, int page = 1)
    {
        const int pageSize = 20;
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(u => u.Nome.Contains(q) || (u.Email != null && u.Email.Contains(q)));
        var total = await query.CountAsync();
        var users = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Q = q;
        return View("~/Views/Admin/Customers/Index.cshtml", users);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("equipe")]
    public async Task<IActionResult> Equipe(string? q)
    {
        var users = await _db.Users
            .Where(u => !string.IsNullOrEmpty(u.Email) &&
                        (string.IsNullOrWhiteSpace(q) || u.Email.Contains(q) || u.Nome.Contains(q) || u.Sobrenome.Contains(q)))
            .OrderByDescending(u => u.CreatedAt)
            .Take(200)
            .ToListAsync();

        var vm = new List<AdminTeamMemberViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin") && !roles.Contains("Manager"))
                continue;

            vm.Add(new AdminTeamMemberViewModel
            {
                UserId = user.Id,
                Nome = user.NomeCompleto,
                Email = user.Email,
                Ativo = user.Ativo,
                Permissao = roles.Contains("Admin") ? "Admin" : "Manager",
                CreatedAt = user.CreatedAt
            });
        }

        ViewBag.Q = q;
        return View("~/Views/Admin/Team/Index.cshtml", vm);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("equipe/{id}/permissao"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarPermissao(string id, string permissao, bool ativo)
    {
        if (permissao is not ("Admin" or "Manager"))
        {
            TempData["Error"] = "Permissão inválida.";
            return RedirectToAction("Equipe");
        }

        if (!await _roleManager.RoleExistsAsync(permissao))
            await _roleManager.CreateAsync(new IdentityRole(permissao));

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Usuário não encontrado.";
            return RedirectToAction("Equipe");
        }

        if (user.Email != null && string.Equals(user.Email, "admin@essenzstore.com.br", StringComparison.OrdinalIgnoreCase) && permissao != "Admin")
        {
            TempData["Error"] = "O administrador principal não pode perder a permissão Admin.";
            return RedirectToAction("Equipe");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removableRoles = currentRoles.Where(r => r is "Admin" or "Manager").ToList();
        if (removableRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, removableRoles);

        await _userManager.AddToRoleAsync(user, permissao);
        user.Ativo = ativo;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Permissão da equipe atualizada!";
        return RedirectToAction("Equipe");
    }

    // ── Financeiro & Operação ───────────────────────────────────────────────
    [HttpGet("financeiro")]
    public async Task<IActionResult> Financeiro()
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var inicio7 = hoje.AddDays(-7);
        var inicio30 = hoje.AddDays(-30);

        var vm = new AdminFinanceiroViewModel
        {
            Faturamento7Dias = await _db.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Approved && o.CreatedAt >= inicio7)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            Faturamento30Dias = await _db.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Approved && o.CreatedAt >= inicio30)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            FaturamentoMesAtual = await _db.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Approved && o.CreatedAt >= inicioMes)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            TotalReembolsosMes = await _db.Orders
                .Where(o => o.CreatedAt >= inicioMes && o.PaymentStatus == PaymentStatus.Refunded)
                .SumAsync(o => (decimal?)o.Total) ?? 0,
            PedidosPagosMes = await _db.Orders
                .CountAsync(o => o.PaymentStatus == PaymentStatus.Approved && o.CreatedAt >= inicioMes),
            PedidosAguardandoEnvio = await _db.Orders
                .CountAsync(o => o.PaymentStatus == PaymentStatus.Approved && (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Separating || o.Status == OrderStatus.Packed)),
            DevolucoesPendentes = await _db.Orders
                .CountAsync(o => o.Status == OrderStatus.Returned)
        };

        vm.VendasPorMetodo = await _db.Payments
            .Where(p => p.Status == PaymentStatus.Approved && p.CreatedAt >= inicio30)
            .GroupBy(p => p.Metodo)
            .Select(g => new AdminMetodoPagamentoViewModel
            {
                Metodo = g.Key.ToString(),
                Pedidos = g.Select(x => x.OrderId).Distinct().Count(),
                Valor = g.Sum(x => x.Valor)
            })
            .OrderByDescending(x => x.Valor)
            .ToListAsync();

        vm.EstoqueCritico = await _db.ProductVariants
            .Where(v => v.Ativo && v.Estoque <= 3)
            .OrderBy(v => v.Estoque)
            .Take(15)
            .Select(v => new AdminEstoqueCriticoViewModel
            {
                Sku = v.Sku ?? "—",
                Produto = v.Product != null ? v.Product.Nome : "Produto sem nome",
                Estoque = v.Estoque
            })
            .ToListAsync();

        var ultimaVendaPorProduto = await _db.OrderItems
            .Where(i => i.Order != null && i.Order.PaymentStatus == PaymentStatus.Approved)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, UltimaVenda = g.Max(x => x.Order!.CreatedAt) })
            .ToDictionaryAsync(x => x.ProductId, x => x.UltimaVenda);

        var produtosAtivos = await _db.Products
            .Include(p => p.Variants)
            .Where(p => p.Ativo)
            .Select(p => new
            {
                p.Id,
                p.Nome,
                EstoqueTotal = p.Variants.Where(v => v.Ativo).Sum(v => (int?)v.Estoque) ?? 0
            })
            .ToListAsync();

        vm.ProdutosSemVenda = produtosAtivos
            .Select(p =>
            {
                var teveVenda = ultimaVendaPorProduto.TryGetValue(p.Id, out var ultimaVenda);
                var diasSemVenda = teveVenda ? (hoje - ultimaVenda.Date).Days : 999;
                return new AdminProdutoSemVendaViewModel
                {
                    ProdutoId = p.Id,
                    Nome = p.Nome,
                    EstoqueTotal = p.EstoqueTotal,
                    DiasSemVenda = diasSemVenda
                };
            })
            .Where(x => x.EstoqueTotal > 0 && x.DiasSemVenda >= 30)
            .OrderByDescending(x => x.DiasSemVenda)
            .ThenBy(x => x.Nome)
            .Take(20)
            .ToList();

        return View("~/Views/Admin/Finance/Index.cshtml", vm);
    }

    // ── Configurações ─────────────────────────────────────────────────────────
    [HttpGet("configuracoes")]
    public async Task<IActionResult> Configuracoes()
    {
        var settings = await _db.StoreSettings.FirstOrDefaultAsync() ?? new StoreSettings();
        return View("~/Views/Admin/Settings/Index.cshtml", settings);
    }

    [HttpPost("configuracoes"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Configuracoes(StoreSettings vm)
    {
        var settings = await _db.StoreSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            _db.StoreSettings.Add(vm);
        }
        else
        {
            settings.NomeLoja = vm.NomeLoja; settings.Slogan = vm.Slogan;
            settings.LogoUrl = vm.LogoUrl; settings.FaviconUrl = vm.FaviconUrl;
            settings.EmailContato = vm.EmailContato; settings.Whatsapp = vm.Whatsapp;
            settings.Instagram = vm.Instagram; settings.Facebook = vm.Facebook;
            settings.Tiktok = vm.Tiktok; settings.Cnpj = vm.Cnpj;
            settings.FreteGratisValor = vm.FreteGratisValor;
            settings.CupomPrimeiraCompra = vm.CupomPrimeiraCompra;
            settings.PercentualPrimeiraCompra = vm.PercentualPrimeiraCompra;
            settings.MetaDescription = vm.MetaDescription;
            settings.GaTrackingId = vm.GaTrackingId; settings.MetaPixelId = vm.MetaPixelId;
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Configurações salvas!";
        return RedirectToAction("Configuracoes");
    }

    // ── Conteúdo (Páginas + FAQ) ───────────────────────────────────────────────
    [HttpGet("conteudo")]
    public async Task<IActionResult> Conteudo()
    {
        ViewBag.Pages = await _db.StorePages.OrderBy(p => p.Titulo).ToListAsync();
        ViewBag.Faqs = await _db.FaqItems.OrderBy(f => f.Ordem).ToListAsync();
        return View("~/Views/Admin/Content/Index.cshtml");
    }

    [HttpPost("conteudo/pagina/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarPagina(int Id, string Titulo, string? Slug, string? ConteudoHtml, bool Publicado = false)
    {
        var page = Id > 0 ? await _db.StorePages.FindAsync(Id) ?? new StorePage() : new StorePage();
        page.Titulo = Titulo;
        page.Slug = string.IsNullOrWhiteSpace(Slug) ? GenerateSlug(Titulo) : Slug;
        page.ConteudoHtml = ConteudoHtml ?? "";
        page.Publicado = Publicado;
        page.UpdatedAt = DateTime.UtcNow;
        if (Id == 0) _db.StorePages.Add(page);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Página salva!";
        return RedirectToAction("Conteudo");
    }

    [HttpPost("conteudo/faq/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarFaq(int Id, string Pergunta, string Resposta, int Ordem, bool Ativo = false)
    {
        var faq = Id > 0 ? await _db.FaqItems.FindAsync(Id) ?? new FaqItem() : new FaqItem();
        faq.Pergunta = Pergunta; faq.Resposta = Resposta;
        faq.Ordem = Ordem; faq.Ativo = Ativo;
        if (Id == 0) _db.FaqItems.Add(faq);
        await _db.SaveChangesAsync();
        TempData["Success"] = "FAQ salvo!";
        return RedirectToAction("Conteudo");
    }

    [HttpPost("conteudo/faq/{id:int}/excluir"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirFaq(int id)
    {
        var faq = await _db.FaqItems.FindAsync(id);
        if (faq != null) { _db.FaqItems.Remove(faq); await _db.SaveChangesAsync(); }
        TempData["Success"] = "FAQ removido.";
        return RedirectToAction("Conteudo");
    }

    // ── Variantes ─────────────────────────────────────────────────────────────
    [HttpPost("produtos/{produtoId:int}/variantes/salvar"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarVariante(int produtoId, int varianteId, string? cor, string? tamanho,
        string sku, int estoque, decimal? preco, bool ativo = false)
    {
        var variant = varianteId > 0
            ? await _db.ProductVariants.FindAsync(varianteId) ?? new ProductVariant { ProductId = produtoId }
            : new ProductVariant { ProductId = produtoId };

        variant.Cor = cor; variant.Tamanho = tamanho;
        variant.Sku = string.IsNullOrWhiteSpace(sku) ? $"SKU-{produtoId}-{DateTime.UtcNow.Ticks}" : sku;
        variant.Estoque = estoque; variant.Preco = preco; variant.Ativo = ativo;

        if (varianteId == 0) _db.ProductVariants.Add(variant);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Variante salva!";
        return RedirectToAction("EditarProduto", new { id = produtoId });
    }

    [HttpPost("produtos/{produtoId:int}/variantes/{varianteId:int}/excluir"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirVariante(int produtoId, int varianteId)
    {
        var v = await _db.ProductVariants.FindAsync(varianteId);
        if (v != null && v.ProductId == produtoId) { _db.ProductVariants.Remove(v); await _db.SaveChangesAsync(); }
        TempData["Success"] = "Variante excluída.";
        return RedirectToAction("EditarProduto", new { id = produtoId });
    }

    // ── Imagens ───────────────────────────────────────────────────────────────
    [HttpPost("produtos/{produtoId:int}/upload-imagem"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImagem(int produtoId, List<IFormFile> files)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "products");
        try
        {
            Directory.CreateDirectory(uploadsDir);
        }
        catch
        {
            uploadsDir = Path.Combine(Path.GetTempPath(), "essenzstore", "uploads", "products");
            Directory.CreateDirectory(uploadsDir);
        }

        var ordem = await _db.ProductImages.Where(i => i.ProductId == produtoId).CountAsync();
        foreach (var file in files)
        {
            if (file.Length == 0) continue;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext)) continue;
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            _db.ProductImages.Add(new ProductImage
            {
                ProductId = produtoId,
                Url = $"/uploads/products/{fileName}",
                Ordem = ordem++,
                Destaque = ordem == 1
            });
        }
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("produtos/{produtoId:int}/imagens/{imagemId:int}/excluir"), ValidateAntiForgeryToken]
    public async Task<IActionResult> ExcluirImagem(int produtoId, int imagemId)
    {
        var img = await _db.ProductImages.FindAsync(imagemId);
        if (img != null && img.ProductId == produtoId)
        {
            // Remove arquivo físico se local
            if (img.Url.StartsWith("/uploads/"))
            {
                var path = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            _db.ProductImages.Remove(img);
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Imagem removida.";
        return RedirectToAction("EditarProduto", new { id = produtoId });
    }

    // ── Envio/Rastreamento ────────────────────────────────────────────────────
    [HttpPost("pedidos/{id:int}/envio"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarEnvio(int id, string? transportadora, string? servico,
        string? codigoRastreio, string? urlRastreio, int prazoDias, string? eventoDescricao)
    {
        var order = await _db.Orders.Include(o => o.Shipments).ThenInclude(s => s.TrackingEvents)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        var shipment = order.Shipments.FirstOrDefault() ?? new Shipment { OrderId = id };
        shipment.Transportadora = transportadora; shipment.Servico = servico;
        shipment.CodigoRastreio = codigoRastreio; shipment.UrlRastreio = urlRastreio;
        shipment.PrazoDias = prazoDias;

        if (shipment.Id == 0) { _db.Shipments.Add(shipment); await _db.SaveChangesAsync(); }

        if (!string.IsNullOrWhiteSpace(eventoDescricao))
        {
            _db.TrackingEvents.Add(new TrackingEvent
            {
                ShipmentId = shipment.Id,
                Descricao = eventoDescricao,
                EventDate = DateTime.UtcNow
            });
        }

        // Se tem código de rastreio e pedido foi pago, marcar como enviado
        if (!string.IsNullOrWhiteSpace(codigoRastreio) && order.Status == OrderStatus.Paid)
        {
            order.Status = OrderStatus.Shipped;
            shipment.Status = ShippingStatus.Posted;
            shipment.PostedAt ??= DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Envio atualizado!";
        return RedirectToAction("DetalhePedido", new { id });
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private static string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant()
            .Replace("ã", "a").Replace("â", "a").Replace("á", "a").Replace("à", "a")
            .Replace("ê", "e").Replace("é", "e").Replace("è", "e")
            .Replace("í", "i").Replace("ì", "i")
            .Replace("ô", "o").Replace("ó", "o").Replace("ò", "o").Replace("õ", "o")
            .Replace("ú", "u").Replace("ù", "u")
            .Replace("ç", "c").Replace("ñ", "n");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        return slug.Trim('-');
    }
}
