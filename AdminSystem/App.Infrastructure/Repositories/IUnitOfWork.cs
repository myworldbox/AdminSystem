using AdminSystem.Application.ViewModels;
using System;

namespace AdminSystem.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<InfoViewModel> Customers { get; }
        IRepository<ContactViewModel> Contacts { get; }
        IRepository<BankViewModel> Banks { get; }
        int Save();
    }
}