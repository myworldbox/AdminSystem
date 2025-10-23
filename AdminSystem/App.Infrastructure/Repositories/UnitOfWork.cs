using AdminSystem.Application.ViewModels;
using AdminSystem.Domain.Entities;
using AdminSystem.Infrastructure.Data;

namespace AdminSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed = false;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Infos = new Repository<客戶資料>(_context);
            Contacts = new Repository<客戶聯絡人>(_context);  // custom repo
            Banks = new Repository<客戶銀行資訊>(_context);
        }

        public IRepository<客戶資料> Infos { get; }
        public IRepository<客戶聯絡人> Contacts { get; }
        public IRepository<客戶銀行資訊> Banks { get; }

        public int Save()
        {
            return _context.SaveChanges();
        }

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
    }
}
