using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yort.Zip.InStore.Tests
{
	[TestClass]
	public class ApplyDefaultTests
	{

		#region Create Order Request Defaults

		[TestMethod]
		public void CreateOrderRequest_IgnoresNullZipConfig()
		{
			var request = new CreateOrderRequest() { Order = new ZipOrder() };
			request.ApplyDefaults(null);
		}

		[TestMethod]
		public void CreateOrderRequest_AppliesStoreIdDefault()
		{
			var config = CreateZipConfig();

			var request = new CreateOrderRequest() { Order = new ZipOrder() };
			request.ApplyDefaults(config);

			Assert.AreEqual(config.DefaultStoreId, request.StoreId);
		}

		[TestMethod]
		public void CreateOrderRequest_AppliesOperatorDefault()
		{
			var config = CreateZipConfig();

			var request = new CreateOrderRequest() { Order = new ZipOrder() };
			request.ApplyDefaults(config);

			Assert.AreEqual(config.DefaultOperator, request.Order.Operator);
		}

		[TestMethod]
		public void CreateOrderRequest_IgnoresNullOrder_WhenApplyingDefaults()
		{
			var config = CreateZipConfig();

			var request = new CreateOrderRequest() { };
			request.ApplyDefaults(config);

			Assert.IsNull(request.Order);
		}

		[TestMethod]
		public void CreateOrderRequest_AppliesTerminalIdDefault()
		{
			var config = CreateZipConfig();

			var request = new CreateOrderRequest() { Order = new ZipOrder() };
			request.ApplyDefaults(config);

			Assert.AreEqual(config.DefaultTerminalId, request.TerminalId);
		}

		#endregion

		#region Refund Order Request Defaults

		[TestMethod]
		public void RefundOrderRequest_IgnoresNullZipConfig()
		{
			var request = new RefundOrderRequest();
			request.ApplyDefaults(null);
		}

		[TestMethod]
		public void RefundOrderRequest_AppliesStoreIdDefault()
		{
			var config = CreateZipConfig();

			var request = new RefundOrderRequest();
			request.ApplyDefaults(config);

			Assert.AreEqual(config.DefaultStoreId, request.StoreId);
		}

		[TestMethod]
		public void RefundOrderRequest_AppliesOperatorDefault()
		{
			var config = CreateZipConfig();

			var request = new RefundOrderRequest();
			request.ApplyDefaults(config);

			Assert.AreEqual(config.DefaultOperator, request.Operator);
		}

		[TestMethod]
		public void RefundOrderRequest_AppliesTerminalIdDefault()
		{
			var config = CreateZipConfig();

			var request = new RefundOrderRequest();
			request.ApplyDefaults(config);

			Assert.AreEqual(config.DefaultTerminalId, request.TerminalId);
		}

		#endregion

		#region Enrol Refund Request Defaults

		[TestMethod]
		public void EnrolRequest_IgnoresNullZipConfig()
		{
			var request = new EnrolRequest();
			request.ApplyDefaults(null);
		}

		[TestMethod]
		public void EnrolRequest_AppliesTerminalIdDefault()
		{
			var config = CreateZipConfig();

			var request = new EnrolRequest();
			request.ApplyDefaults(config);

			Assert.AreEqual(config.DefaultTerminalId, request.Terminal);
		}

		#endregion

		private ZipClientConfiguration CreateZipConfig()
		{
			return new ZipClientConfiguration
			(
				"Test",
				"Test",
				ZipEnvironment.NewZealand.Test
			)
			{
				DefaultStoreId = "Albany",
				DefaultOperator = "Kermit The Frog",
				DefaultTerminalId = "2531"
			};
		}
	}
}
