using System.Data;
using Npgsql;

namespace GestaoRH.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private IDbConnection _connection;
    private IDbTransaction _transaction;
    private bool _disposed;

    private IEmpresaRepository?     _empresaRepository;
    private ISetorRepository?       _setorRepository;
    private IFuncionarioRepository? _funcionarioRepository;

    public UnitOfWork(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public IEmpresaRepository     EmpresaRepository     => _empresaRepository     ??= new EmpresaRepository(_transaction);
    public ISetorRepository       SetorRepository       => _setorRepository       ??= new SetorRepository(_transaction);
    public IFuncionarioRepository FuncionarioRepository => _funcionarioRepository ??= new FuncionarioRepository(_transaction);

    public async Task CommitAsync()
    {
        try
        {
            await ((NpgsqlTransaction)_transaction).CommitAsync();
        }
        catch
        {
            await ((NpgsqlTransaction)_transaction).RollbackAsync();
            throw;
        }
        finally
        {
            _transaction.Dispose();
        }
    }

    public void Rollback()
    {
        _transaction.Rollback();
        _transaction.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }

    ~UnitOfWork()
    {
        Dispose(false);
    }
}
