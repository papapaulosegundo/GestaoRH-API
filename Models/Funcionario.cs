namespace GestaoRH.Models;

public class Funcionario
{
    public int Id { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;   // masculino | feminino | sem_genero
    public string Turno { get; set; } = string.Empty;    // matutino | vespertino | noturno
    public int SetorId { get; set; }
    public string? NomeSetor { get; set; }               // vem do JOIN, não é coluna
    public string SenhaTemporaria { get; set; } = string.Empty;  // visível só para RH
    public string Senha { get; set; } = string.Empty;             // hash BCrypt
    public bool SenhaTrocada { get; set; } = false;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
