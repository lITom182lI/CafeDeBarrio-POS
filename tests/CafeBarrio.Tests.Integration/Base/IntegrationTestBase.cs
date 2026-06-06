using CafeBarrio.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace CafeBarrio.Tests.Integration.Base;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly CafeBarrioDbContext Db;
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;

    protected IntegrationTestBase()
    {
        var options = new DbContextOptionsBuilder<CafeBarrioDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=CafeDeBarrioTest;User Id=sa;Password=Muis_CafeBarrio_2026!;TrustServerCertificate=True;")
            .Options;

        Db = new CafeBarrioDbContext(options);
        
        Db.Database.OpenConnection();
        _connection = Db.Database.GetDbConnection();
        _transaction = _connection.BeginTransaction();
        Db.Database.UseTransaction(_transaction as System.Data.Common.DbTransaction);
    }

    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _connection?.Close();
        Db?.Dispose();
    }
}
