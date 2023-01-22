﻿using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MyOnlineShop.Data;
using MyOnlineShop.Services;
using MyOnlineShop.Models;
using MyOnlineShop.Models.apimodel;
using Microsoft.CodeAnalysis;

namespace MyOnlineShop.Controllers
{
	public class ProfileController : ControllerBase
	{
		private MyShopContext _context;
		public ProfileController(MyShopContext context)
		{
			_context = context;

		}
		[HttpGet]
		[Route("profile/")]
		public ActionResult GetProfiles()
		{
			try
			{
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);

				if (user == null)
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				else
				{
					profileModel p = new profileModel()
					{
						id = user.ID,
						username = user.UserName,
						firstName = user.FirstName,
						lastName = user.LastName,
						phoneNumber = user.PhoneNumber,
						email = user.Email,
						profileImage = user.ImageUrl,
						birthDate = user.BirthDate,
						accessLevel = user.AccessLevel,
						restricted = user.Restricted
					};
					return Ok(p);
				}
			}
			catch { return StatusCode(StatusCodes.Status500InternalServerError); }
		}


		[HttpPut]
		[Route("profile/")]
		public ActionResult putProfile(putprofileModel p)
		{
            Logger logger = new Logger(_context);
            try
			{
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);

				if (user == null)
				{
					logger.LoggerFunc("profile/", 
								p, Unauthorized(), User);
					return Unauthorized();
				}
				var emailcheck = _context.users.SingleOrDefault(s => s.Email == p.email && s.UserName != username);
				var phonecheck = _context.users.SingleOrDefault(s => s.PhoneNumber == p.phoneNumber && s.UserName != username);
				if (emailcheck != null || phonecheck != null)
				{
					logger.LoggerFunc("profile/", 
								p, Forbid(), User);
					return Forbid();
				}
				if (!ModelState.IsValid)
				{
					logger.LoggerFunc("profile/", 
								p, BadRequest(ModelState), User);
					return BadRequest(ModelState);
				}
				else
				{
					user.FirstName = p.firstName;
					user.LastName = p.lastName;
					user.PhoneNumber = p.phoneNumber;
					user.Email = p.email;
					user.ImageUrl = p.profileImage;
					user.BirthDate = p.birthDate;
					_context.Update(user);
					_context.SaveChanges();
					profileModel p1 = new profileModel()
					{
						id = user.ID,
						username = user.UserName,
						firstName = user.FirstName,
						lastName = user.LastName,
						phoneNumber = user.PhoneNumber,
						email = user.Email,
						profileImage = user.ImageUrl,
						birthDate = user.BirthDate,
						accessLevel = user.AccessLevel,
						restricted = user.Restricted
					};
					logger.LoggerFunc("profile/", 
								p, p1, User);
					return Ok(p1);
				}
			}
			catch
			{
				logger.LoggerFunc("profile/", 
								p, StatusCode(StatusCodes.Status500InternalServerError), User);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}


		[HttpGet]
		[Route("profile/carts")]
		public ActionResult profilecart(int page, int cartsPerPage)
		{
			try
			{
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);
				var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
				if (user == null)
				{
					return Unauthorized();
				}
				if (user.AccessLevel.ToLower() != "customer")
				{
					return Forbid();

				}

				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				else
				{
					var carts = _context.cart.Where(c => c.CustomerID == customer.ID).ToList();
					var length = carts.Count();

					var totalPages = (int)Math.Ceiling(Convert.ToDecimal(length) / Convert.ToDecimal(cartsPerPage));
					page = Math.Min(totalPages, page);
					var start = Math.Max((page - 1) * cartsPerPage, 0);
					var end = Math.Min(page * cartsPerPage, length);
					var count = Math.Max(end - start, 0);
					cartsPerPage = count;
					carts = carts.GetRange(start, count);


					var carts1 = new List<eachCart>();
					foreach (var c in carts)
					{
						var orders = _context.orders.Where(s => s.CartID == c.ID).ToList();
						var products = new List<eachproduct>();
						foreach (var o in orders)
						{
							var productprice = _context.productPrices.SingleOrDefault(p => p.ID == o.ProductPriceID);
							eachproduct eachproduct = new eachproduct() { amount = o.Amount, productId = productprice.ProductID };
							products.Add(eachproduct);
						}
						eachCart c1 = new eachCart()
						{
							id = c.ID,
							customerId = c.CustomerID,
							description = c.Description,
							status = c.Status,
							updateDate = c.UpdateDate,
							products = products
						};
						carts1.Add(c1);

					}
					var p = new Pagination<eachCart>()
					{
						page = page,
						perPage = cartsPerPage,
						totalPages = totalPages,
						data = carts1
					};
					return Ok(p);
				}
			}
			catch { return StatusCode(StatusCodes.Status500InternalServerError); }
		}




		[HttpGet]
		[Route("profile/carts/{id:Guid}")]
		public ActionResult profilecart(Guid id)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);
				var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
				if (username == null)
				{
					return Unauthorized();
				}
				if (user.AccessLevel.ToLower() != "customer")
				{
					return Forbid();
				}
				var checkcart = _context.cart.SingleOrDefault(c => c.ID == id && c.CustomerID == customer.ID);
				if (checkcart == null)
				{
					return NotFound();
				}
				else
				{
					var orders = _context.orders.Where(s => s.CartID == id).ToList();
					var products = new List<eachproduct>();
					foreach (var o in orders)
					{
						var productprice = _context.productPrices.SingleOrDefault(p => p.ID == o.ProductPriceID);
						eachproduct eachproduct = new eachproduct() { amount = o.Amount, productId = productprice.ProductID };
						products.Add(eachproduct);
					}
					eachCart c1 = new eachCart()
					{
						id = checkcart.ID,
						customerId = checkcart.CustomerID,
						description = checkcart.Description,
						status = checkcart.Status,
						updateDate = checkcart.UpdateDate,
						products = products
					};

					return Ok(c1);
				}
			}
			catch { return StatusCode(StatusCodes.Status500InternalServerError); }
		}


		[HttpGet]
		[Route("profile/subscription")]
		public ActionResult GetProductsubsription(int page, int productsPerPage, Guid productId)
		{
			try
			{

				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);
				var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
				var reqproducts = _context.requestedProducts.Where(c => c.UserID == user.ID).ToList();
				if (username == null)
				{
					return Unauthorized();
				}
				if (user.AccessLevel.ToLower() != "customer")
				{
					return Forbid();
				}

				if (productId != default(Guid))
				{
					var checkproduct = _context.Products.SingleOrDefault(p => p.ID == productId);
					if (checkproduct == null)
					{
						return NotFound();
					}
					var a = _context.requestedProducts.SingleOrDefault(c => c.ProductID == productId && c.UserID == user.ID);
					var product = _context.Products.SingleOrDefault(p => p.ID == productId);
					if (a == null)
					{
						return NotFound();
					}
					else
					{
						productModel P = new productModel()
						{
							id = product.ID,
							name = product.Name,
							category = product.Category,
							image = product.Image,
							description = product.Description,
							likes = product.likes,
							dislikes = product.dislikes

						};
						return Ok(P);
					}

				}
				else
				{

					if (reqproducts == null)
					{
						return NotFound();
					}
					else
					{
						var length = reqproducts.Count;

						var totalPages = (int)Math.Ceiling((decimal)length / (decimal)productsPerPage);
						page = Math.Min(totalPages, page);
						var start = Math.Max((page - 1) * productsPerPage, 0);
						var end = Math.Min(page * productsPerPage, length);
						var count = Math.Max(end - start, 0);
						productsPerPage = count;

						reqproducts = reqproducts.GetRange(start, count);


						var products = new List<productModel>();
						foreach (var pro in reqproducts)
						{
							var p1 = _context.Products.SingleOrDefault(p => p.ID == pro.ProductID);
							productModel productModel = new productModel()
							{
								id = p1.ID,
								category = p1.Category,
								image = p1.Image,
								description = p1.Description,
								dislikes = p1.dislikes,
								likes = p1.likes,
								name = p1.Name
							};
							products.Add(productModel);
						}
						var p = new Pagination<productModel>()
						{
							page = page,
							totalPages = totalPages,
							perPage = productsPerPage,
							data = products
						};

						return Ok(p);
					}
				}
			}
			catch
			{
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}


		[HttpPost]
		[Route("profile/subscribe")]
		public ActionResult GetProductsubsrib(Guid productId)
		{
            Logger logger = new Logger(_context);
            try
			{
				if (!ModelState.IsValid)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, BadRequest(ModelState), User);
					return BadRequest(ModelState);
				}
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);
				var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
				var checkproduct = _context.Products.SingleOrDefault(p => p.ID == productId);
				if (productId == null)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, BadRequest(), User);
					return BadRequest();
				}
				if (username == null)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, Unauthorized(), User);
					return Unauthorized();
				}
				if (user.AccessLevel.ToLower() != "customer")
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, Forbid(), User);
					return Forbid();
				}
				if (checkproduct == null)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, NotFound(), User);
					return NotFound();
				}

				var checksub = _context.requestedProducts.SingleOrDefault(r => r.UserID == user.ID && r.ProductID == productId);
				if (checksub == null)
				{
					RequestedProducts product = new RequestedProducts()
					{
						ID = Guid.NewGuid(),
						ProductID = productId,
						UserID = user.ID
					};
					_context.requestedProducts.Add(product);
					_context.SaveChanges();
					var p = new Dictionary<string, string> { { "status", "success" } };
					logger.LoggerFunc("profile/subscribe", 
						productId, p, User);
					return Ok(p);

				}
				else
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, Ok(), User);
					return Ok();

				}
			}
			catch
			{
				logger.LoggerFunc("profile/subscribe", 
								productId, StatusCode(StatusCodes.Status500InternalServerError), User);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}


		[HttpDelete]
		[Route("profile/subscribe")]
		public ActionResult DeleteProductsubsrib(Guid productId)
		{
            Logger logger = new Logger(_context);
            try
			{
				if (!ModelState.IsValid)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, BadRequest(ModelState), User);
					return BadRequest(ModelState);
				}
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);
				var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
				var checkproduct = _context.Products.SingleOrDefault(p => p.ID == productId);

				if (productId == null)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, BadRequest(), User);
					return BadRequest();
				}
				if (username == null)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, Unauthorized(), User);
					return Unauthorized();
				}
				if (user.AccessLevel.ToLower() != "customer")
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, Forbid(), User);
					return Forbid();
				}
				if (checkproduct == null)
				{
					logger.LoggerFunc("profile/subscribe", 
								productId, NotFound(), User);
					return NotFound();
				}
				var checksub = _context.requestedProducts.SingleOrDefault(r => r.UserID == user.ID && r.ProductID == productId);
				if (checksub == null)
				{
					logger.LoggerFunc("profile/subscribe", 
						 productId, NotFound(), User);
					return NotFound();
				}
				else
				{
					_context.requestedProducts.Remove(checksub);
					_context.SaveChanges();
					var p = new Dictionary<string, string> { { "status", "success" } };
					logger.LoggerFunc("profile/subscribe", 
								productId, p, User);
					return Ok(p);
				}


			}
			catch
			{
				logger.LoggerFunc("profile/subscribe", 
								productId, StatusCode(StatusCodes.Status500InternalServerError), User);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}


		[HttpGet]
		[Route("profile/carts/current")]
		public ActionResult profilecartcurrentget()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);
				var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
				if (username == null)
				{
					return Unauthorized();
				}
				if (user.AccessLevel.ToLower() != "customer")
				{
					return Forbid();
				}

				var checkcart = _context.cart.SingleOrDefault(c => c.Status.ToLower() == "filling" && c.CustomerID == customer.ID);
				if (checkcart == null)
				{
					return NotFound();
				}
				else
				{
					var orders = _context.orders.Where(s => s.CartID == checkcart.ID).ToList();
					List<eachproduct> eachproducts = new List<eachproduct>();
					foreach (var o in orders)
					{
						var productId = _context.productPrices.SingleOrDefault(p => p.ID == o.ProductPriceID).ProductID;
						eachproduct eachproduct = new eachproduct() { amount = o.Amount, productId = productId };
						eachproducts.Add(eachproduct);

					}
					eachCart p = new eachCart()
					{
						id = checkcart.ID,
						customerId = checkcart.CustomerID,
						description = checkcart.Description,
						products = eachproducts,
						status = checkcart.Status,
						updateDate = checkcart.UpdateDate
					};
					return Ok(p);

				}
			}
			catch { return StatusCode(StatusCodes.Status500InternalServerError); }
		}


		[HttpPut]
		[Route("profile/carts/current")]
		public ActionResult profilecartcurrentput([FromBody] eachproduct product)
		{
            Logger logger = new Logger(_context);
            try
			{
				if (!ModelState.IsValid)
				{
					logger.LoggerFunc("profile/carts/current", 
								product, BadRequest(ModelState), User);
					return BadRequest(ModelState);
				}
				else
				{
					var username = User.FindFirstValue(ClaimTypes.Name);
					var user = _context.users.SingleOrDefault(s => s.UserName == username);
					var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
					if (username == null)
					{
						logger.LoggerFunc("profile/carts/current", 
								product, Unauthorized(), User);
						return Unauthorized();
					}
					if (user.AccessLevel.ToLower() != "customer")
					{
						logger.LoggerFunc("profile/carts/current", 
								product, Forbid(), User);
						return Forbid();
					}

					var checkcart = _context.cart.SingleOrDefault(c => c.Status.ToLower() == "filling" && c.CustomerID == customer.ID);
					if (checkcart == null)
					{
						var cart = new Cart()
						{
							CustomerID = customer.ID,
							ID = Guid.NewGuid(),
							TotalPrice = 0,
							Status = "filling",
							UpdateDate = DateTime.Now,
							Description = "customers cart"
						};
						_context.cart.Add(cart);
						_context.SaveChanges();
						checkcart = cart;
					}

					{
						var productprice = _context.productPrices.SingleOrDefault(p => p.ID == product.productId);
						if (productprice == null)
						{
							logger.LoggerFunc("profile/carts/current", 
								product, Forbid(), User);
							return NotFound();
						}
						var product1 = _context.Products.SingleOrDefault(p => p.ID == productprice.ProductID);
						if (product.amount > productprice.Amount)
						{
							logger.LoggerFunc("profile/carts/current", 
								product, NotFound(), User);
							return NotFound();
						}
						var checkorder = _context.orders.SingleOrDefault(c => c.CartID == checkcart.ID && c.ProductPriceID == product.productId);

						if (checkorder != null)
						{
							checkorder.Amount = checkorder.Amount + product.amount;

							_context.Update(checkorder);
							_context.SaveChanges();
						}
						else
						{
							Order order = new Order()
							{
								ID = Guid.NewGuid(),
								Amount = product.amount,
								ProductPriceID = productprice.ID,
								CartID = checkcart.ID

							};
							_context.orders.Add(order);
							_context.SaveChanges();
						}
						checkcart.TotalPrice = checkcart.TotalPrice + (product.amount * productprice.Price);
						_context.cart.Update(checkcart);
						_context.SaveChanges();
						var orders = _context.orders.Where(s => s.CartID == checkcart.ID).ToList();
						List<eachproduct> eachproducts = new List<eachproduct>();
						foreach (var o in orders)
						{
							var productId = _context.productPrices.SingleOrDefault(p => p.ID == o.ProductPriceID).ProductID;
							eachproduct eachproduct = new eachproduct() { amount = o.Amount, productId = productId };
							eachproducts.Add(eachproduct);

						}
						eachCart p = new eachCart()
						{
							id = checkcart.ID,
							customerId = checkcart.CustomerID,
							description = checkcart.Description,
							products = eachproducts,
							status = checkcart.Status,
							updateDate = checkcart.UpdateDate
						};
						logger.LoggerFunc("profile/carts/current", 
								product, p, User);
						return Ok(p);

					}
				}
			}
			catch
			{
				logger.LoggerFunc("profile/carts/current", 
								product, StatusCode(StatusCodes.Status500InternalServerError), User);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}


		[HttpDelete]
		[Route("profile/carts/current")]
		public ActionResult profilecartcurrentdelete()
		{
            Logger logger = new Logger(_context);
            try
			{
				if (!ModelState.IsValid)
				{
					logger.LoggerFunc("profile/carts/current", 
								null, BadRequest(ModelState), User);
					return BadRequest(ModelState);
				}
				var username = User.FindFirstValue(ClaimTypes.Name);
				var user = _context.users.SingleOrDefault(s => s.UserName == username);
				var customer = _context.customer.SingleOrDefault(c => c.UserId == user.ID);
				if (username == null)
				{
					logger.LoggerFunc("profile/carts/current", 
								null, Unauthorized(), User);
					return Unauthorized();
				}
				if (user.AccessLevel.ToLower() != "customer")
				{
					logger.LoggerFunc("profile/carts/current", 
								null, Forbid(), User);
					return Forbid();
				}

				var checkcart = _context.cart.SingleOrDefault(c => c.Status.ToLower() == "filling" && c.CustomerID == customer.ID);
				if (checkcart == null)
				{
					logger.LoggerFunc("profile/carts/current", 
								null, NotFound(), User);
					return NotFound();
				}

				else
				{
					var orders = _context.orders.Where(c => c.CartID == checkcart.ID).ToList();
					foreach (var o in orders)
					{
						_context.orders.Remove(o);
						_context.SaveChanges();
					}

					var p = new Dictionary<string, string>() { { "status", "success" } };
					logger.LoggerFunc("profile/carts/current", 
								null, p, User);
					return Ok(p);
				}
			}
			catch
			{
				logger.LoggerFunc("profile/carts/current", 
								null, StatusCode(StatusCodes.Status500InternalServerError), User);
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
		}


	}
}