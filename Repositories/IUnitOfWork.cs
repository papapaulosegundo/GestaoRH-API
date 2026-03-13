namespace GestaoRH.Repositories;

public interface IUnitOfWork : IDisposable
{
    IEmpresaRepository EmpresaRepository { get; }

    Task CommitAsync();
    void Rollback();
}
