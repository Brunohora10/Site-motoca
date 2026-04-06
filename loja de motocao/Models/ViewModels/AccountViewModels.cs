using System.ComponentModel.DataAnnotations;

namespace EssenzStore.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha obrigatória")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    public bool LembrarMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nome obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sobrenome obrigatório")]
    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone obrigatório")]
    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [StringLength(14)]
    public string? Cpf { get; set; }

    [Required(ErrorMessage = "Senha obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha obrigatória")]
    [DataType(DataType.Password)]
    [Compare(nameof(Senha), ErrorMessage = "Senhas não conferem")]
    public string ConfirmarSenha { get; set; } = string.Empty;

    public bool AceitaTermos { get; set; }
    public bool AceitaMarketing { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha obrigatória")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação obrigatória")]
    [DataType(DataType.Password)]
    [Compare(nameof(NovaSenha), ErrorMessage = "Senhas não conferem")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}

public class AccountDashboardViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public List<Order> RecentOrders { get; set; } = new();
    public int TotalPedidos { get; set; }
    public int TotalFavoritos { get; set; }
    public List<Address> Addresses { get; set; } = new();
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Nome obrigatório")]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sobrenome obrigatório")]
    [StringLength(100)]
    public string Sobrenome { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(14)]
    public string? Cpf { get; set; }

    public List<Address> Addresses { get; set; } = new();
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Senha atual obrigatória")]
    [DataType(DataType.Password)]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha obrigatória")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação obrigatória")]
    [DataType(DataType.Password)]
    [Compare(nameof(NovaSenha), ErrorMessage = "Senhas não conferem")]
    public string ConfirmarNovaSenha { get; set; } = string.Empty;
}
