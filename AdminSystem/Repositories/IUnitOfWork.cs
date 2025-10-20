using AdminSystem.Models;
using System;

namespace AdminSystem.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<客戶資料> Customers { get; }
        IGenericRepository<客戶聯絡人> Contacts { get; }
        IGenericRepository<客戶銀行資訊> Banks { get; }
        int Save();
    }
}