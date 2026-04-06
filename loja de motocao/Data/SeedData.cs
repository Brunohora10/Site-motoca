using EssenzStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EssenzStore.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Roles
        string[] roles = ["Admin", "Manager", "Customer"];
        foreach (var role in roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Admin user
        const string adminEmail = "admin@essenzstore.com.br";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Nome = "Admin",
                Sobrenome = "Essenz",
                EmailConfirmed = true,
                Ativo = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Store Settings
        if (!await context.StoreSettings.AnyAsync())
        {
            context.StoreSettings.Add(new StoreSettings
            {
                NomeLoja = "ESSENZ STORE",
                Slogan = "Menos pose, mais essência.",
                EmailContato = "contato@essenzstore.com.br",
                Whatsapp = "5579999999999",
                Instagram = "essenzstoree",
                Cnpj = "00.000.000/0001-00",
                FreteGratisValor = 399m,
                CupomPrimeiraCompra = "PRIMEIRACOMPRA",
                PercentualPrimeiraCompra = 10,
                MetaDescription = "ESSENZ STORE — Moda masculina premium em Aracaju. Camisetas, calças, tênis e acessórios das melhores marcas."
            });
        }

        // Brands
        if (!await context.Brands.AnyAsync())
        {
            var brands = new List<Brand>
            {
                new() { Nome = "Casablanca", Slug = "casablanca", Destaque = true, Ordem = 1, Ativo = true },
                new() { Nome = "Diesel", Slug = "diesel", Destaque = true, Ordem = 2, Ativo = true },
                new() { Nome = "Armani Exchange", Slug = "armani-exchange", Destaque = true, Ordem = 3, Ativo = true },
                new() { Nome = "Moncler", Slug = "moncler", Destaque = true, Ordem = 4, Ativo = true },
                new() { Nome = "Lacoste", Slug = "lacoste", Destaque = true, Ordem = 5, Ativo = true },
                new() { Nome = "Nike", Slug = "nike", Destaque = true, Ordem = 6, Ativo = true },
                new() { Nome = "Hugo Boss", Slug = "hugo-boss", Destaque = false, Ordem = 7, Ativo = true },
                new() { Nome = "Tommy Hilfiger", Slug = "tommy-hilfiger", Destaque = false, Ordem = 8, Ativo = true },
                new() { Nome = "Abercrombie", Slug = "abercrombie", Destaque = false, Ordem = 9, Ativo = true },
                new() { Nome = "Gucci", Slug = "gucci", Destaque = false, Ordem = 10, Ativo = true },
            };
            context.Brands.AddRange(brands);
        }

        // Categories
        if (!await context.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Nome = "Camisetas", Slug = "camisetas", Ordem = 1, Ativo = true, ImagemUrl = "/images/cat-camisetas.jpg" },
                new() { Nome = "Calças", Slug = "calcas", Ordem = 2, Ativo = true, ImagemUrl = "/images/cat-calcas.jpg" },
                new() { Nome = "Bermudas", Slug = "bermudas", Ordem = 3, Ativo = true, ImagemUrl = "/images/cat-bermudas.jpg" },
                new() { Nome = "Tênis", Slug = "tenis", Ordem = 4, Ativo = true, ImagemUrl = "/images/cat-tenis.jpg" },
                new() { Nome = "Bonés", Slug = "bones", Ordem = 5, Ativo = true, ImagemUrl = "/images/cat-bones.jpg" },
                new() { Nome = "Polos", Slug = "polos", Ordem = 6, Ativo = true, ImagemUrl = "/images/cat-polos.jpg" },
                new() { Nome = "Conjuntos", Slug = "conjuntos", Ordem = 7, Ativo = true, ImagemUrl = "/images/cat-conjuntos.jpg" },
                new() { Nome = "Acessórios", Slug = "acessorios", Ordem = 8, Ativo = true, ImagemUrl = "/images/cat-acessorios.jpg" },
            };
            context.Categories.AddRange(categories);
        }

        await context.SaveChangesAsync();

        // Seed sample products after brands and categories exist
        if (!await context.Products.AnyAsync())
        {
            var camiseta = await context.Categories.FirstAsync(c => c.Slug == "camisetas");
            var bermuda = await context.Categories.FirstAsync(c => c.Slug == "bermudas");
            var tenis = await context.Categories.FirstAsync(c => c.Slug == "tenis");
            var polo = await context.Categories.FirstAsync(c => c.Slug == "polos");

            var casablanca = await context.Brands.FirstAsync(b => b.Slug == "casablanca");
            var diesel = await context.Brands.FirstAsync(b => b.Slug == "diesel");
            var lacoste = await context.Brands.FirstAsync(b => b.Slug == "lacoste");
            var nike = await context.Brands.FirstAsync(b => b.Slug == "nike");

            var products = new List<Product>
            {
                new()
                {
                    Nome = "Camiseta Casablanca Tennis Club",
                    Slug = "camiseta-casablanca-tennis-club",
                    BrandId = casablanca.Id,
                    CategoryId = camiseta.Id,
                    DescricaoCurta = "Camiseta exclusiva Casablanca com estampado Tennis Club.",
                    DescricaoCompleta = "<p>Peça premium com estampa exclusiva Casablanca Tennis Club. Malha premium 100% algodão egípcio.</p>",
                    Composicao = "100% Algodão",
                    Modelagem = "Regular Fit",
                    InstrucoesLavagem = "Lavar à mão em água fria",
                    Preco = 349.90m,
                    Ativo = true,
                    Destaque = true,
                    Lancamento = true,
                    MaisVendido = false,
                    Peso = 0.3m,
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "CSB-TC-P", Tamanho = "P", Estoque = 5, Ativo = true },
                        new() { Sku = "CSB-TC-M", Tamanho = "M", Estoque = 8, Ativo = true },
                        new() { Sku = "CSB-TC-G", Tamanho = "G", Estoque = 10, Ativo = true },
                        new() { Sku = "CSB-TC-GG", Tamanho = "GG", Estoque = 4, Ativo = true },
                    }
                },
                new()
                {
                    Nome = "Camiseta Diesel Graphic Premium",
                    Slug = "camiseta-diesel-graphic-premium",
                    BrandId = diesel.Id,
                    CategoryId = camiseta.Id,
                    DescricaoCurta = "Camiseta Diesel com estampa gráfica exclusiva.",
                    DescricaoCompleta = "<p>Camiseta Diesel com grafismo exclusivo. Malha de algodão premium.</p>",
                    Composicao = "100% Algodão",
                    Modelagem = "Oversized",
                    Preco = 299.90m,
                    PrecoPromocional = 249.90m,
                    Ativo = true,
                    Destaque = true,
                    Lancamento = true,
                    MaisVendido = true,
                    Peso = 0.3m,
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "DSL-GP-P", Tamanho = "P", Estoque = 6, Ativo = true },
                        new() { Sku = "DSL-GP-M", Tamanho = "M", Estoque = 10, Ativo = true },
                        new() { Sku = "DSL-GP-G", Tamanho = "G", Estoque = 8, Ativo = true },
                        new() { Sku = "DSL-GP-GG", Tamanho = "GG", Estoque = 3, Ativo = true },
                    }
                },
                new()
                {
                    Nome = "Polo Lacoste Premium Piqué",
                    Slug = "polo-lacoste-premium-pique",
                    BrandId = lacoste.Id,
                    CategoryId = polo.Id,
                    DescricaoCurta = "Camisa polo Lacoste em piqué premium.",
                    DescricaoCompleta = "<p>Polo Lacoste clássica em malha piqué de alta qualidade.</p>",
                    Composicao = "100% Algodão Piqué",
                    Modelagem = "Slim Fit",
                    Preco = 289.90m,
                    Ativo = true,
                    Destaque = true,
                    MaisVendido = true,
                    ProdutoMomento = true,
                    Peso = 0.4m,
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "LAC-PQ-P", Tamanho = "P", Cor = "Branco", Estoque = 4, Ativo = true },
                        new() { Sku = "LAC-PQ-M", Tamanho = "M", Cor = "Branco", Estoque = 7, Ativo = true },
                        new() { Sku = "LAC-PQ-G", Tamanho = "G", Cor = "Branco", Estoque = 9, Ativo = true },
                        new() { Sku = "LAC-PQ-M-PR", Tamanho = "M", Cor = "Preto", Estoque = 5, Ativo = true },
                        new() { Sku = "LAC-PQ-G-PR", Tamanho = "G", Cor = "Preto", Estoque = 6, Ativo = true },
                    }
                },
                new()
                {
                    Nome = "Bermuda Nike Tech Fleece",
                    Slug = "bermuda-nike-tech-fleece",
                    BrandId = nike.Id,
                    CategoryId = bermuda.Id,
                    DescricaoCurta = "Bermuda Nike Tech Fleece, conforto e estilo.",
                    DescricaoCompleta = "<p>Bermuda Nike Tech Fleece com tecnologia de retenção de calor.</p>",
                    Composicao = "83% Algodão, 17% Poliéster",
                    Modelagem = "Tapered",
                    Preco = 199.90m,
                    PrecoPromocional = 169.90m,
                    Ativo = true,
                    Lancamento = true,
                    MaisVendido = true,
                    Peso = 0.4m,
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "NK-TF-P", Tamanho = "P", Cor = "Preto", Estoque = 5, Ativo = true },
                        new() { Sku = "NK-TF-M", Tamanho = "M", Cor = "Preto", Estoque = 8, Ativo = true },
                        new() { Sku = "NK-TF-G", Tamanho = "G", Cor = "Preto", Estoque = 6, Ativo = true },
                        new() { Sku = "NK-TF-GG", Tamanho = "GG", Cor = "Preto", Estoque = 2, Ativo = true },
                    }
                },
                new()
                {
                    Nome = "Camiseta Moncler Logo Bordado",
                    Slug = "camiseta-moncler-logo-bordado",
                    BrandId = await context.Brands.Where(b => b.Slug == "moncler").Select(b => b.Id).FirstOrDefaultAsync(),
                    CategoryId = camiseta.Id,
                    DescricaoCurta = "Camiseta Moncler com logo bordado.",
                    DescricaoCompleta = "<p>Camiseta Moncler com logo bordado no peito, malha premium.</p>",
                    Composicao = "100% Algodão",
                    Modelagem = "Regular Fit",
                    Preco = 489.90m,
                    Ativo = true,
                    Destaque = true,
                    Peso = 0.3m,
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "MCL-LB-M", Tamanho = "M", Cor = "Preto", Estoque = 3, Ativo = true },
                        new() { Sku = "MCL-LB-G", Tamanho = "G", Cor = "Preto", Estoque = 5, Ativo = true },
                        new() { Sku = "MCL-LB-GG", Tamanho = "GG", Cor = "Preto", Estoque = 2, Ativo = true },
                    }
                },
                new()
                {
                    Nome = "Camiseta Armani Exchange Signature",
                    Slug = "camiseta-armani-exchange-signature",
                    BrandId = await context.Brands.Where(b => b.Slug == "armani-exchange").Select(b => b.Id).FirstOrDefaultAsync(),
                    CategoryId = camiseta.Id,
                    DescricaoCurta = "Camiseta Armani Exchange com estampa Signature.",
                    DescricaoCompleta = "<p>Camiseta Armani Exchange estampa icônica Signature, algodão premium.</p>",
                    Composicao = "100% Algodão",
                    Modelagem = "Slim Fit",
                    Preco = 259.90m,
                    PrecoPromocional = 219.90m,
                    Ativo = true,
                    MaisVendido = true,
                    Peso = 0.3m,
                    Variants = new List<ProductVariant>
                    {
                        new() { Sku = "AX-SG-P", Tamanho = "P", Estoque = 7, Ativo = true },
                        new() { Sku = "AX-SG-M", Tamanho = "M", Estoque = 9, Ativo = true },
                        new() { Sku = "AX-SG-G", Tamanho = "G", Estoque = 8, Ativo = true },
                        new() { Sku = "AX-SG-GG", Tamanho = "GG", Estoque = 5, Ativo = true },
                    }
                },
            };

            context.Products.AddRange(products);
        }

        // Banners
        if (!await context.Banners.AnyAsync())
        {
            context.Banners.AddRange(
                new Banner
                {
                    Titulo = "Nova Coleção — Menos pose, mais essência",
                    Subtitulo = "Peças exclusivas para quem sabe o que quer",
                    ImagemDesktop = "/images/banner-hero-1.jpg",
                    ImagemMobile = "/images/banner-hero-1-mobile.jpg",
                    Link = "/produtos",
                    TextoBotao = "VER COLEÇÃO",
                    Posicao = "hero",
                    Ativo = true,
                    Ordem = 1
                },
                new Banner
                {
                    Titulo = "Marcas Premium com Exclusividade",
                    Subtitulo = "Casablanca, Moncler, Diesel e muito mais",
                    ImagemDesktop = "/images/banner-hero-2.jpg",
                    ImagemMobile = "/images/banner-hero-2-mobile.jpg",
                    Link = "/marcas",
                    TextoBotao = "EXPLORAR MARCAS",
                    Posicao = "hero",
                    Ativo = true,
                    Ordem = 2
                }
            );
        }

        // FAQs
        if (!await context.FaqItems.AnyAsync())
        {
            context.FaqItems.AddRange(
                new FaqItem { Pergunta = "Qual o prazo de entrega?", Resposta = "O prazo varia de 3 a 10 dias úteis dependendo da sua região. Após o envio, você receberá o código de rastreio por e-mail.", Ordem = 1, Ativo = true },
                new FaqItem { Pergunta = "Como rastrear meu pedido?", Resposta = "Após o envio, você recebe o código de rastreio por e-mail. Você também pode acompanhar na seção 'Meus Pedidos' da sua conta ou na página 'Rastrear Pedido'.", Ordem = 2, Ativo = true },
                new FaqItem { Pergunta = "Qual a política de trocas?", Resposta = "Aceitamos trocas em até 7 dias após o recebimento. A peça deve estar sem uso, com etiquetas e na embalagem original.", Ordem = 3, Ativo = true },
                new FaqItem { Pergunta = "Quais formas de pagamento são aceitas?", Resposta = "Aceitamos PIX (5% de desconto), cartão de crédito em até 12x e boleto bancário.", Ordem = 4, Ativo = true },
                new FaqItem { Pergunta = "Tenho frete grátis?", Resposta = "Sim! Compras acima de R$ 399 têm frete grátis para todo o Brasil.", Ordem = 5, Ativo = true },
                new FaqItem { Pergunta = "Como funciona a tabela de medidas?", Resposta = "Cada produto tem uma tabela de medidas disponível na página do produto. Em caso de dúvida, entre em contato via WhatsApp para orientação personalizada.", Ordem = 6, Ativo = true }
            );
        }

        // Store Pages
        if (!await context.StorePages.AnyAsync())
        {
            context.StorePages.AddRange(
                new StorePage { Titulo = "Quem Somos", Slug = "quem-somos", Publicado = true, ConteudoHtml = "<h2>ESSENZ STORE</h2><p><strong>Menos pose, mais essência.</strong></p><p>Somos uma loja de moda masculina premium sediada em Aracaju-SE, com envio para todo o Brasil. Nossa missão é trazer peças exclusivas das melhores marcas do mundo, com atendimento 100% online e qualidade garantida.</p><p>Acreditamos que estilo não precisa de pose — precisa de essência. Por isso curadoria cada peça com critério, autenticidade e olhar para quem realmente entende de moda.</p>" },
                new StorePage { Titulo = "Trocas e Devoluções", Slug = "trocas-e-devolucoes", Publicado = true, ConteudoHtml = "<h2>Política de Trocas e Devoluções</h2><h3>Prazo</h3><p>Você tem até 7 dias corridos após o recebimento para solicitar troca ou devolução.</p><h3>Condições</h3><ul><li>Peça sem uso e sem avarias</li><li>Etiquetas originais intactas</li><li>Embalagem original preservada</li></ul><h3>Procedimento</h3><p>Acesse 'Minha Conta → Meus Pedidos' e clique em 'Solicitar Troca/Devolução'. Nossa equipe responderá em até 1 dia útil.</p><h3>Reembolso</h3><p>Após aprovação da devolução, o reembolso é feito em até 7 dias úteis no mesmo método de pagamento utilizado.</p>" },
                new StorePage { Titulo = "Política de Envios", Slug = "politica-de-envios", Publicado = true, ConteudoHtml = "<h2>Política de Envios</h2><h3>Prazo de Postagem</h3><p>Os pedidos são postados em até 2 dias úteis após a confirmação do pagamento.</p><h3>Frete Grátis</h3><p>Compras acima de R$ 399,00 têm frete grátis para todo o Brasil.</p><h3>Prazos de Entrega</h3><p>Os prazos variam de 3 a 10 dias úteis dependendo da sua localização. Capitais costumam ser mais rápidas.</p><h3>Rastreamento</h3><p>Após o envio, você receberá o código de rastreio por e-mail e WhatsApp. Acompanhe em tempo real na área do cliente.</p>" },
                new StorePage { Titulo = "Política de Privacidade", Slug = "politica-de-privacidade", Publicado = true, ConteudoHtml = "<h2>Política de Privacidade</h2><p>A ESSENZ STORE está comprometida com a proteção dos seus dados pessoais, em conformidade com a Lei Geral de Proteção de Dados (LGPD - Lei nº 13.709/2018).</p><h3>Dados Coletados</h3><p>Coletamos nome, e-mail, telefone, CPF e endereço para processamento de pedidos.</p><h3>Uso dos Dados</h3><p>Seus dados são utilizados exclusivamente para: processamento de pedidos, comunicação sobre compras, melhoria da experiência e marketing (mediante consentimento).</p><h3>Seus Direitos</h3><p>Você pode solicitar acesso, correção ou exclusão dos seus dados a qualquer momento pelo e-mail privacidade@essenzstore.com.br.</p>" },
                new StorePage { Titulo = "Termos de Uso", Slug = "termos-de-uso", Publicado = true, ConteudoHtml = "<h2>Termos de Uso</h2><p>Ao acessar e utilizar a ESSENZ STORE, você concorda com os presentes Termos de Uso.</p><h3>Uso da Plataforma</h3><p>O site é de uso pessoal e não comercial. É vedado o uso de robôs, scrapers ou qualquer automação não autorizada.</p><h3>Responsabilidade</h3><p>A ESSENZ STORE não se responsabiliza por danos decorrentes do mau uso da plataforma ou por informações incorretas fornecidas pelo usuário.</p><h3>Propriedade Intelectual</h3><p>Todo o conteúdo do site (textos, imagens, layout) é propriedade da ESSENZ STORE e protegido por direitos autorais.</p>" }
            );
        }

        // Coupons
        if (!await context.Coupons.AnyAsync())
        {
            context.Coupons.AddRange(
                new Coupon
                {
                    Codigo = "PRIMEIRACOMPRA",
                    Tipo = CouponType.Percentual,
                    Valor = 10,
                    ValorMinimo = 100,
                    Ativo = true,
                    Descricao = "10% de desconto na primeira compra",
                    UsoPorCliente = 1
                },
                new Coupon
                {
                    Codigo = "ESSENZ15",
                    Tipo = CouponType.Percentual,
                    Valor = 15,
                    ValorMinimo = 200,
                    Ativo = true,
                    Descricao = "15% de desconto para clientes VIP"
                }
            );
        }

        // Imagens de produtos (demo)
        if (!await context.ProductImages.AnyAsync())
        {
            var imageMap = new Dictionary<string, string[]>
            {
                ["camiseta-casablanca-tennis-club"] = ["/images/products/p1.jpg", "/images/products/p2.jpg"],
                ["camiseta-diesel-graphic-premium"] = ["/images/products/p3.jpg"],
                ["polo-lacoste-premium-pique"] = ["/images/products/p4.jpg", "/images/products/p5.jpg"],
                ["bermuda-nike-tech-fleece"] = ["/images/products/p6.jpg"],
                ["camiseta-moncler-logo-bordado"] = ["/images/products/p7.jpg"],
                ["camiseta-armani-exchange-signature"] = ["/images/products/p8.jpg"]
            };

            var productsBySlug = await context.Products
                .Where(p => imageMap.Keys.Contains(p.Slug))
                .ToDictionaryAsync(p => p.Slug, p => p.Id);

            foreach (var entry in imageMap)
            {
                if (!productsBySlug.TryGetValue(entry.Key, out var productId)) continue;

                var order = 0;
                foreach (var imageUrl in entry.Value)
                {
                    context.ProductImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        Url = imageUrl,
                        Ordem = order,
                        Destaque = order == 0
                    });
                    order++;
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
