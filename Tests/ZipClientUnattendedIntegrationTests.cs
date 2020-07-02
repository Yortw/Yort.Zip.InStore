using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yort.Zip.InStore;

namespace Tests
{
	[TestClass]
	public class ZipClientUnattendedIntegrationTests
	{
		[TestMethod]
		public async Task ZipClient_CanCreateOrder()
		{
			var client = new ZipClient(ZipEnvironment.NewZealand.Test);

			var request = new CreateOrderRequest()
			{
				Order = new NewZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = "AA05",
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = "Test",
					PaymentFlow = ZipPaymentFlow.Payment
				}
			};

			var result = await client.CreateOrderAsync(request);
			Assert.IsNotNull(result);
			Assert.IsFalse(String.IsNullOrWhiteSpace(result.OrderId));
		}

		[TestMethod]
		public async Task ZipClient_CanGetOrderStatus()
		{
			var client = new ZipClient(ZipEnvironment.NewZealand.Test);

			#region Create an order to get the status of

			var request = new CreateOrderRequest()
			{
				Order = new NewZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = "AA20",
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = "Test",
					PaymentFlow = ZipPaymentFlow.Payment
				}
			};

			var createOrderResult = await client.CreateOrderAsync(request);
			Assert.IsNotNull(createOrderResult);

			#endregion


			var statusResponse = await client.GetOrderStatusAsync(new OrderStatusRequest() { OrderId = createOrderResult.OrderId });
			Assert.IsNotNull(statusResponse);

			while (!ZipOrderStatus.IsTerminalStatus(statusResponse.Status))
			{
				System.Diagnostics.Trace.WriteLine($"Order {statusResponse.OrderNumber} status is {statusResponse.Status}");
				await Task.Delay(1000);
				statusResponse = await client.GetOrderStatusAsync(new OrderStatusRequest() { OrderId = createOrderResult.OrderId });
			}

			Assert.IsNotNull(statusResponse);
		}

	}
}
