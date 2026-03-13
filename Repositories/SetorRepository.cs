using System.Data;
using Dapper;
using GestaoRH.Models;

namespace GestaoRH.Repositories;

public class SetorRepository : ISetorRepository
{
    private readonly IDbTransaction _transaction;
    private IDbConnection Connection => _transaction.Connection!;

    public SetorRepository(IDbTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<int> CriarAsync(Setor setor)
    {
        const string sql = @"
            INSERT INTO setor (nome, descricao, ativo, criado_em)
            VALUES (@Nome, @Descricao, @Ativo, @CriadoEm)
            RETURNING id";

        return await Connection.ExecuteScalarAsync<int>(sql, setor, transaction: _transaction);
    }

    public async Task<Setor?> ObterPorIdAsync(int id)
    {
        const string sql = @"
            SELECT
                id         AS ""Id"",
                nome       AS ""Nome"",
                descricao  AS ""Descricao"",
                ativo      AS ""Ativo"",
                criado_em  AS ""CriadoEm""
            FROM setor
            WHERE id = @Id";

        return await Connection.QueryFirstOrDefaultAsync<Setor>(
            sql, new { Id = id }, transaction: _transaction);
    }

    public async Task<Setor?> ObterPorNomeAsync(string nome)
    {
        const string sql = @"
            SELECT
                id         AS ""Id"",
                nome       AS ""Nome"",
                descricao  AS ""Descricao"",
                ativo      AS ""Ativo"",
                criado_em  AS ""CriadoEm""
            FROM setor
            WHERE LOWER(nome) = LOWER(@Nome)";

        return await Connection.QueryFirstOrDefaultAsync<Setor>(
            sql, new { Nome = nome }, transaction: _transaction);
    }

    public async Task AtualizarAsync(Setor setor)
    {
        const string sql = @"
            UPDATE setor
            SET nome = @Nome, descricao = @Descricao
            WHERE id = @Id";

        await Connection.ExecuteAsync(sql, setor, transaction: _transaction);
    }

    public async Task DesativarAsync(int id)
    {
        const string sql = @"UPDATE setor SET ativo = false WHERE id = @Id";
        await Connection.ExecuteAsync(sql, new { Id = id }, transaction: _transaction);
    }

    public async Task<IEnumerable<Setor>> ListarAsync()
    {
        const string sql = @"
            SELECT
                id         AS ""Id"",
                nome       AS ""Nome"",
                descricao  AS ""Descricao"",
                ativo      AS ""Ativo"",
                criado_em  AS ""CriadoEm""
            FROM setor
            WHERE ativo = true
            ORDER BY nome";

        return await Connection.QueryAsync<Setor>(sql, transaction: _transaction);
    }
}
