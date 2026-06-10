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
    private static readonly object _dbLock = new();
    private static bool _dbInitialized;

    protected IntegrationTestBase()
    {
        var localFallback = "Server=localhost,1434;Database=CafeDeBarrioTest;Integrated Security=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<CafeBarrioDbContext>()
            .UseSqlServer(
                Environment.GetEnvironmentVariable("TEST_DB_CONNECTION") ?? localFallback)
            .Options;

        Db = new CafeBarrioDbContext(options);

        lock (_dbLock)
        {
            if (!_dbInitialized)
            {
                Db.Database.EnsureDeleted(); // Para limpiar el estado sucio del test fallido anterior
                Db.Database.EnsureCreated();
                _dbInitialized = true;
            }
        }

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
