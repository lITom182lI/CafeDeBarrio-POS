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
        var saPassword = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Muis_CafeBarrio_2026!";
        var localFallback = $"Server=localhost,1433;Database=CafeDeBarrioTest;User Id=sa;Password={saPassword};TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<CafeBarrioDbContext>()
            .UseSqlServer(
                Environment.GetEnvironmentVariable("TEST_DB_CONNECTION") ?? localFallback)
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
