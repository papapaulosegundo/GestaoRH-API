using GestaoRH.Models;

namespace GestaoRH.Repositories;

public interface IFuncionarioRepository
{
    Task<int> CriarAsync(Funcionario funcionario);
    Task<Funcionario?> ObterPorIdAsync(int id);
    Task<Funcionario?> ObterPorCpfAsync(string cpf);
    Task<Funcionario?> ObterPorEmailAsync(string email);
    Task AtualizarAsync(Funcionario funcionario);
    Task AtualizarSenhaAsync(int id, string senhaHash);
    Task DesativarAsync(int id);
    Task<IEnumerable<Funcionario>> ListarAsync();
    Task<IEnumerable<Funcionario>> ListarPorSetorAsync(int setorId);
}
