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
            Contacts = new Repository<ContactViewModel>(_context);  // custom repo
            Banks = new Repository<BankViewModel>(_context);
        }

        public IRepository<InfoViewModel> Customers { get; }
        public IRepository<ContactViewModel> Contacts { get; }
        public IRepository<BankViewModel> Banks { get; }

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
