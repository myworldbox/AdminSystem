using AdminSystem.Models;
using System;
using System.Linq;

namespace AdminSystem.Repositories
{
    public class ContactRepository : GenericRepository<客戶聯絡人>
    {
        public ContactRepository(CustomerEntities context) : base(context) { }

        public override void Insert(客戶聯絡人 entity)
        {
            if (Get(filter: c => c.客戶Id == entity.客戶Id && c.Email == entity.Email).Any())
                throw new InvalidOperationException("同客戶下Email不能重複");
            base.Insert(entity);
        }

        public override void Update(客戶聯絡人 entity)
        {
            if (Get(filter: c => c.客戶Id == entity.客戶Id && c.Email == entity.Email && c.Id != entity.Id).Any())
                throw new InvalidOperationException("同客戶下Email不能重複");
            base.Update(entity);
        }
    }
}