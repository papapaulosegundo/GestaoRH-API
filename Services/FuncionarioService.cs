using GestaoRH.Models;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;

namespace GestaoRH.Services;

public class FuncionarioService
{
    private readonly IUnitOfWork _uof;

    private static readonly HashSet<string> GenerosValidos = new(StringComparer.OrdinalIgnoreCase)
        { "masculino", "feminino", "sem_genero" };

    private static readonly HashSet<string> TurnosValidos = new(StringComparer.OrdinalIgnoreCase)
        { "matutino", "vespertino", "noturno" };

    public FuncionarioService(IUnitOfWork uof)
    {
        _uof = uof;
    }

    // ─── Geração de senha temporária ────────────────────────────────────────
    // Regra: 4 primeiros dígitos do CPF + "senha#"
    // Ex: CPF 123.456.789-00  →  senha "1234senha#"
    private static string GerarSenhaTemporaria(string cpf)
    {
        var soDigitos = new string(cpf.Where(char.IsDigit).ToArray());
        var prefixo   = soDigitos.Length >= 4 ? soDigitos[..4] : soDigitos;
        return $"{prefixo}senha#";
    }

    // ─── Cadastro ────────────────────────────────────────────────────────────
    public async Task<Funcionario> Cadastrar(FuncionarioCadastroDto dto)
    {
        // Validações básicas
        if (string.IsNullOrWhiteSpace(dto.Cpf)   ||
            string.IsNullOrWhiteSpace(dto.Nome)   ||
            string.IsNullOrWhiteSpace(dto.Email)  ||
            dto.SetorId <= 0)
            throw new ArgumentException("CPF, Nome, Email e Setor são obrigatórios.");

        if (!GenerosValidos.Contains(dto.Genero))
            throw new ArgumentException("Gênero inválido. Use: masculino, feminino ou sem_genero.");

        if (!TurnosValidos.Contains(dto.Turno))
            throw new ArgumentException("Turno inválido. Use: matutino, vespertino ou noturno.");

        // CPF duplicado?
        var cpfExiste = await _uof.FuncionarioRepository.ObterPorCpfAsync(dto.Cpf);
        if (cpfExiste != null)
            throw new InvalidOperationException("CPF já cadastrado.");

        // Email duplicado?
        var emailExiste = await _uof.FuncionarioRepository.ObterPorEmailAsync(dto.Email);
        if (emailExiste != null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        // Setor existe?
        var setor = await _uof.SetorRepository.ObterPorIdAsync(dto.SetorId);
        if (setor == null)
            throw new KeyNotFoundException("Setor informado não encontrado.");

        // Gera senha temporária
        var senhaTemp = GerarSenhaTemporaria(dto.Cpf);
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaTemp);

        var funcionario = new Funcionario
        {
            Cpf              = dto.Cpf,
            Nome             = dto.Nome,
            Telefone         = dto.Telefone,
            Email            = dto.Email,
            Genero           = dto.Genero.ToLower(),
            Turno            = dto.Turno.ToLower(),
            SetorId          = dto.SetorId,
            SenhaTemporaria  = senhaTemp,   // texto claro — visível só para RH
            Senha            = senhaHash,
            SenhaTrocada     = false,
            Ativo            = true,
            CriadoEm        = DateTime.UtcNow
        };

        var id = await _uof.FuncionarioRepository.CriarAsync(funcionario);
        return await _uof.FuncionarioRepository.ObterPorIdAsync(id)
               ?? throw new Exception("Falha ao recuperar funcionário após cadastro.");
    }

    // ─── Login pelo CPF ──────────────────────────────────────────────────────
    public async Task<Funcionario> Login(string cpf, string senha)
    {
        var funcionario = await _uof.FuncionarioRepository.ObterPorCpfAsync(cpf);

        if (funcionario == null || !funcionario.Ativo)
            throw new UnauthorizedAccessException("CPF ou senha inválidos.");

        if (!BCrypt.Net.BCrypt.Verify(senha, funcionario.Senha))
            throw new UnauthorizedAccessException("CPF ou senha inválidos.");

        return funcionario;
    }

    // ─── Troca de senha pelo próprio funcionário ─────────────────────────────
    public async Task TrocarSenha(int id, FuncionarioTrocarSenhaDto dto)
    {
        var funcionario = await _uof.FuncionarioRepository.ObterPorIdAsync(id)
                          ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.SenhaAtual, funcionario.Senha))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        if (string.IsNullOrWhiteSpace(dto.NovaSenha) || dto.NovaSenha.Length < 6)
            throw new ArgumentException("A nova senha deve ter pelo menos 6 caracteres.");

        var novoHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
        await _uof.FuncionarioRepository.AtualizarSenhaAsync(id, novoHash);
    }

    // ─── Consultas ───────────────────────────────────────────────────────────
    public async Task<Funcionario> ObterPorId(int id)
    {
        return await _uof.FuncionarioRepository.ObterPorIdAsync(id)
               ?? throw new KeyNotFoundException("Funcionário não encontrado.");
    }

    public async Task<IEnumerable<Funcionario>> Listar()
        => await _uof.FuncionarioRepository.ListarAsync();

    public async Task<IEnumerable<Funcionario>> ListarPorSetor(int setorId)
        => await _uof.FuncionarioRepository.ListarPorSetorAsync(setorId);

    // ─── Atualização ─────────────────────────────────────────────────────────
    public async Task<Funcionario> Atualizar(int id, FuncionarioAtualizarDto dto)
    {
        var funcionario = await _uof.FuncionarioRepository.ObterPorIdAsync(id)
                          ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        if (!GenerosValidos.Contains(dto.Genero))
            throw new ArgumentException("Gênero inválido.");

        if (!TurnosValidos.Contains(dto.Turno))
            throw new ArgumentException("Turno inválido.");

        // Verifica se novo email já pertence a outro funcionário
        if (!string.Equals(funcionario.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailExiste = await _uof.FuncionarioRepository.ObterPorEmailAsync(dto.Email);
            if (emailExiste != null)
                throw new InvalidOperationException("E-mail já está em uso por outro funcionário.");
        }

        funcionario.Nome     = dto.Nome;
        funcionario.Telefone = dto.Telefone;
        funcionario.Email    = dto.Email;
        funcionario.Genero   = dto.Genero.ToLower();
        funcionario.Turno    = dto.Turno.ToLower();
        funcionario.SetorId  = dto.SetorId;

        await _uof.FuncionarioRepository.AtualizarAsync(funcionario);
        return funcionario;
    }

    public async Task Desativar(int id)
    {
        _ = await _uof.FuncionarioRepository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException("Funcionário não encontrado.");

        await _uof.FuncionarioRepository.DesativarAsync(id);
    }

    // ─── Mapeamento para DTO ─────────────────────────────────────────────────
    public static FuncionarioResponseDto ToResponse(Funcionario f) => new()
    {
        Id           = f.Id,
        Cpf          = f.Cpf,
        Nome         = f.Nome,
        Telefone     = f.Telefone,
        Email        = f.Email,
        Genero       = f.Genero,
        Turno        = f.Turno,
        SetorId      = f.SetorId,
        NomeSetor    = f.NomeSetor,
        SenhaTrocada = f.SenhaTrocada,
        Ativo        = f.Ativo,
        CriadoEm    = f.CriadoEm
    };

    /// <summary>Inclui senha temporária em texto — somente para o RH</summary>
    public static FuncionarioRhResponseDto ToRhResponse(Funcionario f) => new()
    {
        Id               = f.Id,
        Cpf              = f.Cpf,
        Nome             = f.Nome,
        Telefone         = f.Telefone,
        Email            = f.Email,
        Genero           = f.Genero,
        Turno            = f.Turno,
        SetorId          = f.SetorId,
        NomeSetor        = f.NomeSetor,
        SenhaTrocada     = f.SenhaTrocada,
        Ativo            = f.Ativo,
        CriadoEm        = f.CriadoEm,
        SenhaTemporaria  = f.SenhaTemporaria
    };
}
