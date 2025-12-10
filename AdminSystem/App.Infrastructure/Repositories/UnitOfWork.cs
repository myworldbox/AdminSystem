// File: Infrastructure/Repositories/UnitOfWork.cs

using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AdminSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private bool _disposed = false;

    // Lazy initialization = better for testing & future extensions
    private Repository<客戶資料>? _infos;
    private Repository<客戶聯絡人>? _contacts;
    private Repository<客戶銀行資訊>? _banks;

    public UnitOfWork(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<客戶資料> Infos =>
        _infos ??= new Repository<客戶資料>(_context);

    public IRepository<客戶聯絡人> Contacts =>
        _contacts ??= new Repository<客戶聯絡人>(_context);

    public IRepository<客戶銀行資訊> Banks =>
        _banks ??= new Repository<客戶銀行資訊>(_context);

    // Sync save (keep for compatibility)
    public int Save()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveAsync()
        => await _context.SaveChangesAsync();

    // Async save - THIS IS THE ONE YOU SHOULD USE IN CONTROLLERS/SERVICES
    public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    #region Dispose Pattern

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            await _context.DisposeAsync();
            _disposed = true;
        }
    }

    #endregion
}