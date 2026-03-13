using GestaoRH.Models;
using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;

namespace GestaoRH.Services;

public class SetorService
{
    private readonly IUnitOfWork _uof;

    public SetorService(IUnitOfWork uof)
    {
        _uof = uof;
    }

    public async Task<Setor> Cadastrar(SetorCadastroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new ArgumentException("Nome do setor é obrigatório.");

        var jaExiste = await _uof.SetorRepository.ObterPorNomeAsync(dto.Nome);
        if (jaExiste != null)
            throw new InvalidOperationException($"Já existe um setor com o nome '{dto.Nome}'.");

        var setor = new Setor
        {
            Nome      = dto.Nome.Trim(),
            Descricao = dto.Descricao.Trim(),
            Ativo     = true,
            CriadoEm = DateTime.UtcNow
        };

        var id = await _uof.SetorRepository.CriarAsync(setor);
        return await _uof.SetorRepository.ObterPorIdAsync(id)
               ?? throw new Exception("Falha ao recuperar setor após cadastro.");
    }

    public async Task<Setor> ObterPorId(int id)
    {
        return await _uof.SetorRepository.ObterPorIdAsync(id)
               ?? throw new KeyNotFoundException("Setor não encontrado.");
    }

    public async Task<IEnumerable<Setor>> Listar()
    {
        return await _uof.SetorRepository.ListarAsync();
    }

    public async Task<Setor> Atualizar(int id, SetorAtualizarDto dto)
    {
        var setor = await _uof.SetorRepository.ObterPorIdAsync(id)
                    ?? throw new KeyNotFoundException("Setor não encontrado.");

        // Verifica duplicata apenas se o nome mudou
        if (!string.Equals(setor.Nome, dto.Nome, StringComparison.OrdinalIgnoreCase))
        {
            var duplicado = await _uof.SetorRepository.ObterPorNomeAsync(dto.Nome);
            if (duplicado != null)
                throw new InvalidOperationException($"Já existe um setor com o nome '{dto.Nome}'.");
        }

        setor.Nome      = dto.Nome.Trim();
        setor.Descricao = dto.Descricao.Trim();

        await _uof.SetorRepository.AtualizarAsync(setor);
        return setor;
    }

    public async Task Desativar(int id)
    {
        var setor = await _uof.SetorRepository.ObterPorIdAsync(id)
                    ?? throw new KeyNotFoundException("Setor não encontrado.");

        await _uof.SetorRepository.DesativarAsync(id);
    }

    public static SetorResponseDto ToResponse(Setor s) => new()
    {
        Id        = s.Id,
        Nome      = s.Nome,
        Descricao = s.Descricao,
        Ativo     = s.Ativo,
        CriadoEm = s.CriadoEm
    };
}
