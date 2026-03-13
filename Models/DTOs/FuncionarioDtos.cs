namespace GestaoRH.Models.DTOs;

public class FuncionarioCadastroDto
{
    public string Cpf { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;   // masculino | feminino | sem_genero
    public string Turno { get; set; } = string.Empty;    // matutino | vespertino | noturno
    public int SetorId { get; set; }
}

public class FuncionarioAtualizarDto
{
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string Turno { get; set; } = string.Empty;
    public int SetorId { get; set; }
}

public class FuncionarioLoginDto
{
    public string Cpf { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class FuncionarioTrocarSenhaDto
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}

/// <summary>Retorno padrão — sem expor hash da senha</summary>
public class FuncionarioResponseDto
{
    public int Id { get; set; }
    public string Cpf { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string Turno { get; set; } = string.Empty;
    public int SetorId { get; set; }
    public string? NomeSetor { get; set; }
    public bool SenhaTrocada { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}

/// <summary>Retorno para o RH — inclui a senha temporária em texto claro</summary>
public class FuncionarioRhResponseDto : FuncionarioResponseDto
{
    public string SenhaTemporaria { get; set; } = string.Empty;
}
