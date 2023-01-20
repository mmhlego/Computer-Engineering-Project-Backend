﻿namespace MyOnlineShop.Models.apimodel
{
	public class PostPrice
	{

		public Guid SellerID { get; set; }
		public Guid ProductID { get; set; }
		public int Amount { get; set; }
		public Double Price { get; set; }
		public String Discount { get; set; }
	}

	public class PutPrice
	{
		public int Amount { get; set; }
		public Double Price { get; set; }
		public String Discount { get; set; }
	}


	public class priceModel
	{
		public Guid id { get; set; }
		public SellerSchema Seller { get; set; }
		public Guid productId { get; set; }
		public int amount { get; set; }
		public double price { get; set; }
		public string priceHistory { get; set; }
		public string discount { get; set; }

	}

}

