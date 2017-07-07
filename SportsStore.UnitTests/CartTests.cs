using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Entities;
using SportsStore.WebUI.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SportsStore.WebUI.Models;
using SportsStore.WebUI.HtmlHelpers;

namespace SportsStore.UnitTests
{
	[TestClass]
	public class CartTests
	{
		[TestMethod]
		public void Can_Add_New_Lines()
		{
			// arrange
			Product p1 = new Product { ProductID = 1, Name = "P1" };
			Product p2 = new Product { ProductID = 2, Name = "P2" };

			Cart target = new Cart();

			// act
			target.AddItem(p1, 1);
			target.AddItem(p2, 1);
			CartLine[] results = target.Lines.ToArray();

			// assert
			Assert.AreEqual(results.Length, 2);
			Assert.AreEqual(results[0].Product, p1);
			Assert.AreEqual(results[1].Product, p2);
		}

		[TestMethod]
		public void Can_Add_Quantity_For_Lines()
		{
			// arrange
			Product p1 = new Product { ProductID = 1, Name = "P1" };
			Product p2 = new Product { ProductID = 2, Name = "P2" };

			Cart target = new Cart();

			// act
			target.AddItem(p1, 1);
			target.AddItem(p2, 1);
			target.AddItem(p1, 10);
			CartLine[] results = target.Lines.OrderBy(c => c.Product.ProductID).ToArray();

			// assert
			Assert.AreEqual(results.Length, 2);
			Assert.AreEqual(results[0].Quantity, 11);
			Assert.AreEqual(results[1].Quantity, 1);
		}

		[TestMethod]
		public void Can_Remove_Line()
		{
			// arrange
			Product p1 = new Product { ProductID = 1, Name = "P1" };
			Product p2 = new Product { ProductID = 2, Name = "P2" };
			Product p3 = new Product { ProductID = 3, Name = "P3" };

			Cart target = new Cart();

			// act
			target.AddItem(p1, 1);
			target.AddItem(p2, 3);
			target.AddItem(p3, 5);
			target.AddItem(p1, 1);
			target.RemoveLine(p2);

			// assert
			Assert.AreEqual(target.Lines.Where(c => c.Product == p2).Count(), 0);
			Assert.AreEqual(target.Lines.Count(), 2);
		}

		[TestMethod]
		public void Calculate_Cart_Total()
		{
			// arrange
			Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100m };
			Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50m };

			Cart target = new Cart();

			// act
			target.AddItem(p1, 1);
			target.AddItem(p2, 1);
			target.AddItem(p1, 3);
			decimal result = target.ComputeTotalValue();

			// assert
			Assert.AreEqual(result, 450m);
		}

		[TestMethod]
		public void Can_Clear_Contents()
		{
			// arrange
			Product p1 = new Product { ProductID = 1, Name = "P1", Price = 100m };
			Product p2 = new Product { ProductID = 2, Name = "P2", Price = 50m };

			Cart target = new Cart();

			// act
			target.AddItem(p1, 1);
			target.AddItem(p2, 1);
			target.Clear();

			// assert
			Assert.AreEqual(target.Lines.Count(), 0);
		}

		[TestMethod]
		public void Can_Add_To_Cart()
		{
			Mock<IProductRepository> mock = new Mock<IProductRepository>();
			mock.Setup(m => m.Products).Returns(new Product[]
			{
				new Product { ProductID = 1, Name = "P1", Category = "Apples" },
			}.AsQueryable());
			// arrange

			Cart cart = new Cart();
			CartController target = new CartController(mock.Object, null);

			// act
			target.AddToCart(cart, 1, null);

			// assert
			Assert.AreEqual(cart.Lines.Count(), 1);
			Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
		}

		[TestMethod]
		public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
		{
			Mock<IProductRepository> mock = new Mock<IProductRepository>();
			mock.Setup(m => m.Products).Returns(new Product[]
			{
				new Product { ProductID = 1, Name = "P1", Category = "Apples" },
			}.AsQueryable());

			// arrange
			Cart cart = new Cart();
			CartController target = new CartController(mock.Object, null);

			// act
			RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");

			// assert
			Assert.AreEqual(result.RouteValues["action"], "Index");
			Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
		}

		[TestMethod]
		public void Can_View_Cart_Contents()
		{
			// arrange
			Cart cart = new Cart();
			CartController target = new CartController(null, null);

			// act
			CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

			// assert
			Assert.AreSame(result.Cart, cart);
			Assert.AreEqual(result.ReturnUrl, "myUrl");
		}

		[TestMethod]
		public void Cannot_Checkout_Empty_Cart()
		{
			// Arrange - create a mock order processor
			Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
			// Arrange - create an empty cart
			Cart cart = new Cart();
			// Arrange - create shipping details
			ShippingDetails shippingDetails = new ShippingDetails();
			// Arrange - create an instance of the controller
			CartController target = new CartController(null, mock.Object);
			// Act
			ViewResult result = target.Checkout(cart, shippingDetails);
			// Assert - check that the order hasn't been passed on to the processor
			mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());
			// Assert - check that the method is returning the default view
			Assert.AreEqual("", result.ViewName);
			// Assert - check that I am passing an invalid model to the view
			Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
		}

		[TestMethod]
		public void Cannot_Checkout_Invalid_ShippingDetails()
		{
			// Arrange - create a mock order processor
			Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
			// Arrange - create a cart with an item
			Cart cart = new Cart();
			cart.AddItem(new Product(), 1);
			// Arrange - create an instance of the controller
			CartController target = new CartController(null, mock.Object);
			// Arrange - add an error to the model
			target.ModelState.AddModelError("error", "error");
			// Act - try to checkout
			ViewResult result = target.Checkout(cart, new ShippingDetails());
			// Assert - check that the order hasn't been passed on to the processor
			mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());
			// Assert - check that the method is returning the default view
			Assert.AreEqual("", result.ViewName);
			// Assert - check that I am passing an invalid model to the view
			Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
		}

		[TestMethod]
		public void Can_Checkout_And_Submit_Order()
		{
			// Arrange - create a mock order processor
			Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
			// Arrange - create a cart with an item
			Cart cart = new Cart();
			cart.AddItem(new Product(), 1);
			// Arrange - create an instance of the controller
			CartController target = new CartController(null, mock.Object);
			// Act - try to checkout
			ViewResult result = target.Checkout(cart, new ShippingDetails());
			// Assert - check that the order has been passed on to the processor
			mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once());
			// Assert - check that the method is returning the Completed view
			Assert.AreEqual("Completed", result.ViewName);
			// Assert - check that I am passing a valid model to the view
			Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
		}
	}
}
