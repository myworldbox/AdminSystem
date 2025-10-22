using AdminSystem.Application.ViewModels;
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
            Customers = new Repository<InfoViewModel>(_context);
            Contacts = new ContactRepository(_context);  // custom repo
            Banks = new Repository<UserBankViewModel>(_context);
        }

        public IRepository<InfoViewModel> Customers { get; }
        public IRepository<UserContactViewModel> Contacts { get; }
        public IRepository<UserBankViewModel> Banks { get; }

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
