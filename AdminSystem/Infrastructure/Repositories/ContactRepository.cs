using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using System;
using System.Linq;

namespace AdminSystem.Infrastructure.Repositories
{
    public class ContactRepository : Repository<UserContactViewModel>
    {
        public ContactRepository(AppDbContext context) : base(context) { }

        public override void Insert(UserContactViewModel entity)
        {
            if (Get(filter: c => c.客戶Id == entity.客戶Id && c.Email == entity.Email).Any())
                throw new InvalidOperationException("同客戶下Email不能重複");
            base.Insert(entity);
        }

        public override void Update(UserContactViewModel entity)
        {
            if (Get(filter: c => c.客戶Id == entity.客戶Id && c.Email == entity.Email && c.Id != entity.Id).Any())
                throw new InvalidOperationException("同客戶下Email不能重複");
            base.Update(entity);
        }
    }
}