﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOnlineShop.Data;
using MyOnlineShop.Services;
using MyOnlineShop.Models.apimodel;
using MyOnlineShop.Models;

namespace MyOnlineShop.Controllers
{
	public class discountTokensController : ControllerBase
	{

		private MyShopContext _context;
		public discountTokensController(MyShopContext context)
		{
			_context = context;

		}

		[HttpGet]
		[Route("discountTokens/{id}/Validate")]
		public ActionResult discountTokenGet(Guid id)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var s = new Dictionary<string, string>();
				var token = _context.tokens.Where(t => t.Id == id).Single();

				if (token != null)
				{
					if (DateTime.Now < token.ExpirationDate)
					{
						s = new Dictionary<string, string>() { { "status", "Valid" } };
					}
					else
					{
						s = new Dictionary<string, string>() { { "status", "InValid" } };
					}

				}
				else
				{
					s = new Dictionary<string, string>() { { "status", "InValid" } };

				}

				return Ok(s);
			}
			catch
			{
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}

		[HttpPut]
		[Route("discountTokens/{id}/use")]

		public ActionResult discountTokenPost(Guid id, Guid cartId)
		{


			var username = User.FindFirstValue(ClaimTypes.Name);
			if (username == null)
			{
				return Unauthorized();
			}
			var userId = _context.users.SingleOrDefault(u => u.UserName == username).ID;
			var customer = _context.customer.SingleOrDefault(c => c.UserId == userId);
			if (customer == null)
			{
				return Forbid();
			}
			Cart cart = new Cart();
			if (cartId == default(Guid))
			{
				cart = _context.cart.SingleOrDefault(c => c.CustomerID == customer.ID && c.Status.ToLower() == "filling");

			}
			else
			{

				cart = _context.cart.SingleOrDefault(C => C.ID == cartId);
			}
			var token1 = _context.tokens.SingleOrDefault(t => t.Id == id);

			if (cart == null)
			{
				return NotFound();
			}
			if (cart.TotalPrice == 0)
			{
				return BadRequest();
			}


			var status = "Invalid";
			if (token1 != null && DateTime.Now <= token1.ExpirationDate)

			{
				status = "Valid";
				string[] t = token1.Discount.Split(new char[] { '_' });
				if (t.Length == 2 && t[0] == "AMOUNT")
				{
					double a = Convert.ToDouble(t[1]);
					cart.TotalPrice = Math.Max(cart.TotalPrice - a, 0);
				}
				else if (t.Length == 2 && t[0] == "PERCENT")
				{
					double a = Convert.ToDouble(t[1]);
					cart.TotalPrice = (cart.TotalPrice * (100 - a) / 100);
				}
				cart.UpdateDate = DateTime.Now;
				_context.Update(cart);

				_context.SaveChanges();
			}
			var orders = _context.orders.Where(o => o.CartID == cart.ID).ToList();
			var ps = new List<eachproduct>();
			foreach (var o in orders)
			{
				var product = _context.productPrices.SingleOrDefault(p => p.ID == o.ProductPriceID);

				eachproduct p = new eachproduct()
				{
					productId = product.ProductID,
					amount = o.Amount

				};
				ps.Add(p);
			}
			eachCart eachCart = new eachCart()
			{
				customerId = cart.CustomerID,
				description = cart.Description,
				id = cart.ID,
				products = ps,
				status = "Filling",
				updateDate = cart.UpdateDate,
				totalprice = cart.TotalPrice,
			};
			token t1 = new token()
			{
				status = status,
				cart = eachCart
			};
			// Logger.LoggerFunc(User.FindFirstValue(ClaimTypes.Name), "Put", "DiscountToken_Put_by_ID & use");
			return Ok(t1);



		}
	}
}

