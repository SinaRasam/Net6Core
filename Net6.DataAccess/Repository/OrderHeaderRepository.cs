using Net6.DataAccess.Repository.IRepository;
using Net6.Models;
using Net6.Models.ViewModels;
using Net6Core.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net6.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            this.db = db;
        }
        public void Update(OrderHeader obj)
        {
           db.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = db.OrderHeaders.FirstOrDefault(u=>u.Id==id);
            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if(paymentStatus != null)
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }
        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDb = db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            orderFromDb.PaymentDate= DateTime.Now; 
            orderFromDb.SessionId = sessionId;
            orderFromDb.PaymentIntentId = paymentIntentId;
        }
    }
}
      
