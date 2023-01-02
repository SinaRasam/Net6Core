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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }
        public void Update(Category category)
        {
           db.Categories.Update(category);
        }
    }
}
