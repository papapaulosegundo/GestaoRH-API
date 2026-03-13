using System.Data;
using Dapper;
using GestaoRH.Models;

namespace GestaoRH.Repositories;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly IDbTransaction _transaction;
    private IDbConnection Connection => _transaction.Connection!;

    public FuncionarioRepository(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    // Fragmento de SELECT reutilizado
    private const string SelectBase = @"
        SELECT
            f.id                  AS ""Id"",
            f.cpf                 AS ""Cpf"",
            f.nome                AS ""Nome"",
            f.telefone            AS ""Telefone"",
            f.email               AS ""Email"",
            f.genero              AS ""Genero"",
            f.turno               AS ""Turno"",
            f.setor_id            AS ""SetorId"",
            s.nome                AS ""NomeSetor"",
            f.senha_temporaria    AS ""SenhaTemporaria"",
            f.senha               AS ""Senha"",
            f.senha_trocada       AS ""SenhaTrocada"",
            f.ativo               AS ""Ativo"",
            f.criado_em           AS ""CriadoEm""
        FROM funcionario f
        LEFT JOIN setor s ON s.id = f.setor_id";

    public async Task<int> CriarAsync(Funcionario funcionario)
    {
        const string sql = @"
            INSERT INTO funcionario (
                cpf, nome, telefone, email,
                genero, turno, setor_id,
                senha_temporaria, senha,
                senha_trocada, ativo, criado_em
            ) VALUES (
                @Cpf, @Nome, @Telefone, @Email,
                @Genero, @Turno, @SetorId,
                @SenhaTemporaria, @Senha,
                @SenhaTrocada, @Ativo, @CriadoEm
            ) RETURNING id";

        return await Connection.ExecuteScalarAsync<int>(sql, funcionario, transaction: _transaction);
    }

    public async Task<Funcionario?> ObterPorIdAsync(int id)
    {
        var sql = SelectBase + " WHERE f.id = @Id";
        return await Connection.QueryFirstOrDefaultAsync<Funcionario>(
            sql, new { Id = id }, transaction: _transaction);
    }

    public async Task<Funcionario?> ObterPorCpfAsync(string cpf)
    {
        var sql = SelectBase + " WHERE f.cpf = @Cpf";
        return await Connection.QueryFirstOrDefaultAsync<Funcionario>(
            sql, new { Cpf = cpf }, transaction: _transaction);
    }

    public async Task<Funcionario?> ObterPorEmailAsync(string email)
    {
        var sql = SelectBase + " WHERE f.email = @Email";
        return await Connection.QueryFirstOrDefaultAsync<Funcionario>(
            sql, new { Email = email }, transaction: _transaction);
    }

    public async Task AtualizarAsync(Funcionario funcionario)
    {
        const string sql = @"
            UPDATE funcionario
            SET
                nome     = @Nome,
                telefone = @Telefone,
                email    = @Email,
                genero   = @Genero,
                turno    = @Turno,
                setor_id = @SetorId
            WHERE id = @Id";

        await Connection.ExecuteAsync(sql, funcionario, transaction: _transaction);
    }

    public async Task AtualizarSenhaAsync(int id, string senhaHash)
    {
        const string sql = @"
            UPDATE funcionario
            SET senha = @SenhaHash, senha_trocada = true
            WHERE id = @Id";

        await Connection.ExecuteAsync(sql, new { Id = id, SenhaHash = senhaHash }, transaction: _transaction);
    }

    public async Task DesativarAsync(int id)
    {
        const string sql = @"UPDATE funcionario SET ativo = false WHERE id = @Id";
        await Connection.ExecuteAsync(sql, new { Id = id }, transaction: _transaction);
    }

    public async Task<IEnumerable<Funcionario>> ListarAsync()
    {
        var sql = SelectBase + " WHERE f.ativo = true ORDER BY f.nome";
        return await Connection.QueryAsync<Funcionario>(sql, transaction: _transaction);
    }

    public async Task<IEnumerable<Funcionario>> ListarPorSetorAsync(int setorId)
    {
        var sql = SelectBase + " WHERE f.ativo = true AND f.setor_id = @SetorId ORDER BY f.nome";
        return await Connection.QueryAsync<Funcionario>(
            sql, new { SetorId = setorId }, transaction: _transaction);
    }
}
