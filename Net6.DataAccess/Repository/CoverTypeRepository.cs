using Net6.DataAccess.Repository.IRepository;
using Net6.Models;
using Net6Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net6.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private ApplicationDbContext _db;
        public CoverTypeRepository(ApplicationDbContext db):base(db)
        {
            this._db = db;
        }
        public void Update(CoverType obj)
        {
            _db.Update(obj);
        }
    }
}
