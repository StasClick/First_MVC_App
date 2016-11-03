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
	public class UnitTest1
	{
		[TestMethod]
		public void Can_Paginate()
		{
			// arrange
			Mock<IProductRepository> mock = new Mock<IProductRepository>();
			mock.Setup(m => m.Products).Returns(new Product[]
			{
				new Product {ProductID = 1, Name = "P1" },
				new Product {ProductID = 2, Name = "P2" },
				new Product {ProductID = 3, Name = "P3" },
				new Product {ProductID = 4, Name = "P4" },
				new Product {ProductID = 5, Name = "P5" },
			});
			ProductController controller = new ProductController(mock.Object);
			controller.PageSize = 3;

			// act
			ProductsListViewModel result = (ProductsListViewModel)controller.List(2).Model;

			// assert
			Product[] prodArray = result.Products.ToArray();
			Assert.IsTrue(prodArray.Length == 2);
			Assert.AreEqual(prodArray[0].Name, "P4");
			Assert.AreEqual(prodArray[1].Name, "P5");
		}

		[TestMethod]
		public void Can_Generate_Page_Links()
		{
			// arrange
			HtmlHelper myHelper = null;
			PagingInfo pagingInfo = new PagingInfo
			{
				CurrentPage = 2,
				TotalItems = 28,
				ItemsPerPage = 10,
			};
			Func<int, string> pageUrlDelegate = i => "Page" + i;

			// act
			MvcHtmlString result = myHelper.PageLinks(pagingInfo, pageUrlDelegate);
			// assert
			string a1 =
				@"<a class=""btn btn-default"" href=""Page1"">1</a>" +
				@"<a class=""btn btn-default btn-primary selected"" href=""Page2"">2</a>" +
				@"<a class=""btn btn-default"" href=""Page3"">3</a>";
			string a2 = result.ToString();
			Assert.AreEqual(
a1,
				a2);
		}

		[TestMethod]
		public void Can_Send_Pagination_View_Model()
		{
			// arrange
			Mock<IProductRepository> mock = new Mock<IProductRepository>();
			mock.Setup(m => m.Products).Returns(new Product[]
			{
				new Product {ProductID = 1, Name = "P1" },
				new Product {ProductID = 2, Name = "P2" },
				new Product {ProductID = 3, Name = "P3" },
				new Product {ProductID = 4, Name = "P4" },
				new Product {ProductID = 5, Name = "P5" },
			});
			ProductController controller = new ProductController(mock.Object);
			controller.PageSize = 3;

			// act
			ProductsListViewModel result = (ProductsListViewModel)controller.List(2).Model;

			// assert
			PagingInfo pageInfo = result.PagingInfo;
			Assert.AreEqual(pageInfo.CurrentPage, 2);
			Assert.AreEqual(pageInfo.ItemsPerPage, 3);
			Assert.AreEqual(pageInfo.TotalItems, 5);
			Assert.AreEqual(pageInfo.TotalPages, 2);
		}
	}
}
