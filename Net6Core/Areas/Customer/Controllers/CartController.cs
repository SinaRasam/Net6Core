using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Net6.DataAccess.Repository;
using Net6.DataAccess.Repository.IRepository;
using Net6.Models;
using Net6.Models.ViewModels;
using Net6.Utility;
using Stripe.Checkout;
using System.Security.Claims;

namespace Net6Core.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IEmailSender emailSender;

		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public int OrderTotal { get; set; }

		public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
		{
			this.unitOfWork = unitOfWork;
			this.emailSender = emailSender;
		}
		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartVM()
			{
				ListCart = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
				,
				OrderHeader = new()
			};
			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVM);
		}

		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartVM()
			{
				ListCart = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
		  ,
				OrderHeader = new()
			};
			ShoppingCartVM.OrderHeader.ApplicationUser = unitOfWork.ApplicationUser.GetFirstOrDefault(
				u => u.Id == claim.Value);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVM);
		}

		[HttpPost, ValidateAntiForgeryToken]
		[ActionName("Summary")]
		public IActionResult SummaryPost(/*ShoppingCartVM ShoppingCartVM*/)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM.ListCart = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");
			ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBaseOnQuantity(cart.Count, cart.Product.Price,
					cart.Product.Price50, cart.Product.Price100);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ApplicationUser applicationUser = unitOfWork.ApplicationUser.GetFirstOrDefault(U => U.Id == claim.Value);
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
			{
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}

			unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			unitOfWork.Save();

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count,
				};
				unitOfWork.OrderDetail.Add(orderDetail);
				unitOfWork.Save();
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{

				//Stripe Settings
				var domain = "https://localhost:44388/";
				var options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string>
				{
				  "card",
				},
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + $"customer/cart/index",
				};

				foreach (var item in ShoppingCartVM.ListCart)
				{

					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							},

						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);

				}

				var service = new SessionService();
				Session session = service.Create(options);
				unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}
			else
			{
				return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
			}


		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader orderHeader = unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == id,includeProperties:"ApplicationUser");

			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);

				//check the stripe status
				if (session.PaymentStatus.ToLower() == "paid")
				{
					unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					unitOfWork.Save();
				}
			}

			//emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email,"New Order - Net Core","<p>New Order Created</p>");
			List<ShoppingCart> shoppingCarts = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId ==
			orderHeader.ApplicationUserId).ToList();
			HttpContext.Session.Clear();
			unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
			unitOfWork.Save();

			return View(id);

		}
		public IActionResult Plus(int cartId)
		{
			var cart = unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
			unitOfWork.ShoppingCart.IncrementCount(cart, 1);
			unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int cartId)
		{
			var cart = unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
			if (cart.Count <= 1)
			{
				unitOfWork.ShoppingCart.Remove(cart);
                var count = unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count-1;
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
			else
			{
				unitOfWork.ShoppingCart.DecrementCount(cart, 1);
			}

			unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Remove(int cartId)
		{
			var cart = unitOfWork.ShoppingCart.GetFirstOrDefault(c => c.Id == cartId);
			unitOfWork.ShoppingCart.Remove(cart);
			unitOfWork.Save();
			var count = unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId==cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.SessionCart, count);
            return RedirectToAction(nameof(Index));
		}
		private double GetPriceBaseOnQuantity(double quantity, double price, double price50, double price100)
		{
			if (quantity <= 50)
			{
				return price;
			}
			else if (quantity <= 100)
			{
				return price50;
			}
			else
			{
				return price100;
			}
		}
	}
}

