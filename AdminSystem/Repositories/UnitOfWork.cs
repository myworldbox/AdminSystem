using AdminSystem.Models;

namespace AdminSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CustomerEntities _context;
        private bool _disposed = false;

        public UnitOfWork(CustomerEntities context)
        {
            _context = context;
            Customers = new GenericRepository<客戶資料>(_context);
            Contacts = new ContactRepository(_context);  // custom repo
            Banks = new GenericRepository<客戶銀行資訊>(_context);
        }

        public IGenericRepository<客戶資料> Customers { get; }
        public IGenericRepository<客戶聯絡人> Contacts { get; }
        public IGenericRepository<客戶銀行資訊> Banks { get; }

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
