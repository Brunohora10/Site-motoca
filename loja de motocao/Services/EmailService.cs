using System.Net;
using System.Net.Mail;

namespace EssenzStore.Services;

public interface IEmailService
{
    Task SendWelcomeAsync(string email, string nome);
    Task SendOrderConfirmationAsync(string email, string nome, string numeroPedido);
    Task SendPaymentApprovedAsync(string email, string nome, string numeroPedido);
    Task SendShippingNotificationAsync(string email, string nome, string numeroPedido, string codigoRastreio);
    Task SendForgotPasswordAsync(string email, string nome, string resetLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var host = _config["Email:Host"];
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogWarning("E-mail não configurado. Para: {Email} | Assunto: {Subject}", toEmail, subject);
            return;
        }

        var port = int.Parse(_config["Email:Port"] ?? "587");
        var fromEmail = _config["Email:From"] ?? username;
        var fromName = _config["Email:FromName"] ?? "Essenz Store";
        var enableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "true");

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var msg = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(msg);
            _logger.LogInformation("E-mail enviado → {Email} | {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail para {Email}", toEmail);
        }
    }

    private static string BaseTemplate(string titulo, string conteudo) => $$"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head><meta charset="utf-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/>
        <style>
          body{margin:0;padding:0;background:#0d0d0f;font-family:'Segoe UI',Arial,sans-serif;color:#e5e5e5}
          .wrap{max-width:600px;margin:0 auto;background:#161618;border-radius:12px;overflow:hidden;margin-top:24px;border:1px solid #2a2a2d}
          .header{background:linear-gradient(135deg,#b8960c,#d4af37);padding:32px 40px;text-align:center}
          .header h1{margin:0;font-size:28px;color:#0d0d0f;letter-spacing:4px;font-weight:800}
          .header p{margin:4px 0 0;color:#0d0d0f;opacity:.7;font-size:13px;letter-spacing:2px}
          .body{padding:40px}
          .body h2{color:#d4af37;font-size:20px;margin:0 0 16px}
          .body p{color:#aaa;line-height:1.7;margin:0 0 16px;font-size:15px}
          .box{background:#0d0d0f;border:1px solid #2a2a2d;border-radius:8px;padding:20px 24px;margin:20px 0}
          .box strong{color:#d4af37;font-size:22px}
          .btn{display:inline-block;background:#d4af37;color:#0d0d0f;text-decoration:none;padding:14px 32px;border-radius:8px;font-weight:700;font-size:15px;margin-top:16px}
          .footer{padding:24px 40px;border-top:1px solid #2a2a2d;text-align:center}
          .footer p{color:#555;font-size:12px;margin:4px 0}
          .footer a{color:#d4af37;text-decoration:none}
        </style></head>
        <body>
          <div class="wrap">
            <div class="header"><h1>ESSENZ</h1><p>STORE</p></div>
            <div class="body">
              <h2>{{titulo}}</h2>
              {{conteudo}}
            </div>
            <div class="footer">
              <p>&copy; {{DateTime.Now.Year}} Essenz Store. Todos os direitos reservados.</p>
              <p><a href="https://essenzstore.com.br">essenzstore.com.br</a> | <a href="https://wa.me/5548988163125">WhatsApp</a></p>
            </div>
          </div>
        </body></html>
        """;

    public Task SendWelcomeAsync(string email, string nome) =>
        SendAsync(email, nome, "Bem-vindo à Essenz Store! 🎉",
            BaseTemplate("Olá, " + nome + "!",
                $"""
                <p>Sua conta foi criada com sucesso. Bem-vindo à <strong style="color:#d4af37">Essenz Store</strong> — menos pose, mais essência.</p>
                <p>Explore nossa coleção e aproveite:</p>
                <div class="box">
                  <p style="margin:4px 0;color:#e5e5e5">🚚 Frete grátis acima de R$299</p>
                  <p style="margin:4px 0;color:#e5e5e5">🏷️ Use o cupom <strong style="color:#d4af37">PRIMEIRACOMPRA</strong> para 5% OFF</p>
                  <p style="margin:4px 0;color:#e5e5e5">🔄 Troca fácil em até 30 dias</p>
                </div>
                <a href="https://essenzstore.com.br/Products/Index" class="btn">Ver Coleção</a>
                """));

    public Task SendOrderConfirmationAsync(string email, string nome, string numeroPedido) =>
        SendAsync(email, nome, $"Pedido #{numeroPedido} recebido! ✅",
            BaseTemplate("Pedido Confirmado!",
                $"""
                <p>Olá, <strong>{nome}</strong>! Recebemos seu pedido e já estamos preparando tudo com carinho.</p>
                <div class="box">
                  <p style="margin:0 0 8px;color:#aaa;font-size:13px">Número do pedido</p>
                  <strong>#{numeroPedido}</strong>
                </div>
                <p>Você receberá um novo e-mail assim que seu pedido for despachado com o código de rastreio.</p>
                <a href="https://essenzstore.com.br/minha-conta" class="btn">Acompanhar Pedido</a>
                """));

    public Task SendPaymentApprovedAsync(string email, string nome, string numeroPedido) =>
        SendAsync(email, nome, $"Pagamento aprovado — Pedido #{numeroPedido} 💰",
            BaseTemplate("Pagamento Aprovado!",
                $"""
                <p>Ótima notícia, <strong>{nome}</strong>! Seu pagamento foi confirmado e o pedido está sendo separado.</p>
                <div class="box">
                  <p style="margin:0 0 8px;color:#aaa;font-size:13px">Pedido</p>
                  <strong>#{numeroPedido}</strong>
                </div>
                <p>Em breve você receberá o código de rastreio por e-mail.</p>
                <a href="https://essenzstore.com.br/minha-conta" class="btn">Ver Meus Pedidos</a>
                """));

    public Task SendShippingNotificationAsync(string email, string nome, string numeroPedido, string codigoRastreio) =>
        SendAsync(email, nome, $"Seu pedido #{numeroPedido} foi despachado! 📦",
            BaseTemplate("Pedido Despachado!",
                $"""
                <p>Boas notícias, <strong>{nome}</strong>! Seu pedido já está a caminho.</p>
                <div class="box">
                  <p style="margin:0 0 4px;color:#aaa;font-size:13px">Código de rastreio</p>
                  <strong style="font-size:18px;letter-spacing:2px">{codigoRastreio}</strong>
                </div>
                <a href="https://rastreamento.correios.com.br/app/index.php?objetos={codigoRastreio}" class="btn">Rastrear Pedido</a>
                """));

    public Task SendForgotPasswordAsync(string email, string nome, string resetLink) =>
        SendAsync(email, nome, "Redefinir senha — Essenz Store 🔐",
            BaseTemplate("Redefinir Senha",
                $"""
                <p>Olá, <strong>{nome}</strong>! Recebemos uma solicitação para redefinir a senha da sua conta.</p>
                <p>Clique no botão abaixo para criar uma nova senha. Este link é válido por <strong>24 horas</strong>.</p>
                <a href="{resetLink}" class="btn">Redefinir Minha Senha</a>
                <p style="margin-top:24px;font-size:13px;color:#555">Se você não solicitou a redefinição, ignore este e-mail.</p>
                """));
}
