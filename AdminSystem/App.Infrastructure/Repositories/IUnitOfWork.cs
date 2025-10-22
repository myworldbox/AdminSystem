using AdminSystem.Application.ViewModels;
using System;

namespace AdminSystem.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<InfoViewModel> Customers { get; }
        IRepository<UserContactViewModel> Contacts { get; }
        IRepository<UserBankViewModel> Banks { get; }
        int Save();
    }
}