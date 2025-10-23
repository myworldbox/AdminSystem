using AdminSystem.Domain.Entities;

namespace AdminSystem.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<客戶資料> Infos { get; }
        IRepository<客戶聯絡人> Contacts { get; }
        IRepository<客戶銀行資訊> Banks { get; }
        int Save();
    }
}