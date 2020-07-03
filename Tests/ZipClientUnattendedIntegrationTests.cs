using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yort.Zip.InStore;

namespace Tests
{
	[TestClass]
	public class ZipClientUnattendedIntegrationTests
	{
		[TestMethod]
		public async Task ZipClient_CreateOrder_ThrowsWhenClientDisposed()
		{
			var client = CreateTestClient();
			client.Dispose();

			var request = new CreateOrderRequest()
			{
				TerminalId = "2531",
				Order = new ZipOrder()
				{
					Amount = 10.5M,
					CustomerApprovalCode = "AA05",
					MerchantReference = System.Guid.NewGuid().ToString(),
					Operator = "Test",
					PaymentFlow = ZipPaymentFlow.Payment,
					Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
				}
			};

			await Assert.ThrowsExceptionAsync<ObjectDisposedException>
			(
				async () => { await client.CreateOrderAsync(request); }
			);
		}

		[TestMethod]
		public async Task ZipClient_CanCreateOrder()
		{
			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
					}
				};

				var result = await client.CreateOrderAsync(request);
				Assert.IsNotNull(result);
				Assert.IsFalse(String.IsNullOrWhiteSpace(result.OrderId));
				Assert.IsNotNull(result.OrderExpiry);
			}
		}

		[TestMethod]
		public async Task ZipClient_CanCreateOrder_ThrowsZipApiException_ForErrorResponse()
		{
			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "BM00",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
					}
				};

				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.CreateOrderAsync(request); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_CanCreateOrder_WithExplicitStore()
		{
			//Note: This test requires your test Zip account to support the correct style of auth
			//and have a branch with the correct id.

			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					StoreId = Environment.GetEnvironmentVariable("ZipPayments_TestStoreId"),
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
					}
				};

				var result = await client.CreateOrderAsync(request);
				Assert.IsNotNull(result);
				Assert.IsFalse(String.IsNullOrWhiteSpace(result.OrderId));
				Assert.IsNotNull(result.OrderExpiry);
			}
		}

		[Ignore("Not working, waiting on assistance from Zip.")]
		[TestMethod]
		public async Task ZipClient_CreateOrder_AppliesDuplicateCheckWhenRequested()
		{
			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					EnableUniqueMerchantReferenceCheck = true,
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
					}
				};

				var result1 = await client.CreateOrderAsync(request);
				Assert.IsNotNull(result1);
				Assert.IsFalse(String.IsNullOrWhiteSpace(result1.OrderId));
				Assert.IsNotNull(result1.OrderExpiry);

				var result2 = await client.CreateOrderAsync(request);
				Assert.IsNotNull(result2);
				Assert.IsFalse(String.IsNullOrWhiteSpace(result2.OrderId));
				Assert.IsNotNull(result2.OrderExpiry);

				Assert.AreEqual(result1.OrderId, result2.OrderId);
			}
		}

		[TestMethod]
		public async Task ZipClient_CreateOrder_CreatesDuplicateOrderWhenDuplicateCheckNotRequested()
		{
			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					EnableUniqueMerchantReferenceCheck = false,
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
					}
				};

				var result1 = await client.CreateOrderAsync(request);
				Assert.IsNotNull(result1);
				Assert.IsFalse(String.IsNullOrWhiteSpace(result1.OrderId));
				Assert.IsNotNull(result1.OrderExpiry);

				var result2 = await client.CreateOrderAsync(request);
				Assert.IsNotNull(result2);
				Assert.IsFalse(String.IsNullOrWhiteSpace(result2.OrderId));
				Assert.IsNotNull(result2.OrderExpiry);

				Assert.AreNotEqual(result1.OrderId, result2.OrderId);
			}
		}

		[TestMethod]
		public async Task ZipClient_CanGetOrderStatus()
		{
			using (var client = CreateTestClient())
			{

				#region Create an order to get the status of

				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
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
				Assert.AreEqual(ZipOrderStatus.Complete, statusResponse.Status);
			}
		}

		[TestMethod]
		public async Task ZipClient_GetOrderStatus_ThrowsApiExceptionOnErrorResponse()
		{
			using (var client = CreateTestClient())
			{
				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.GetOrderStatusAsync(new OrderStatusRequest() { OrderId = System.Guid.NewGuid().ToString() }); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_CanCancelOrder()
		{
			using (var client = CreateTestClient())
			{

				#region Create an order to cancel
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "CA30",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment
					}
				};

				var createOrderResult = await client.CreateOrderAsync(request);
				Assert.IsNotNull(createOrderResult);
				Assert.IsFalse(String.IsNullOrWhiteSpace(createOrderResult.OrderId));

				#endregion

				var cancelResponse = await client.CancelOrderAsync(new CancelOrderRequest() { OrderId = createOrderResult.OrderId, Operator = "Test", TerminalId = "2531" });
				Assert.IsNotNull(cancelResponse);
				Assert.AreEqual(createOrderResult.OrderId, cancelResponse.OrderId);
			}
		}

		[TestMethod]
		public async Task ZipClient_CancelOrder_ThrowsApiExceptionOnErrorResponse()
		{
			using (var client = CreateTestClient())
			{
				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.CancelOrderAsync(new CancelOrderRequest() { OrderId = System.Guid.NewGuid().ToString(), Operator = "Test", TerminalId = "2531" }); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_CanRefundOrder()
		{
			// So according to Zip you can't refund an order using a pre-canned approval code. You must manually login to the test consumer, generate a pre-approval code, and then apply that here for the refund to succeed :(
			// In order to work around this, we've created a sale for a very large value and this test refunds only a single dollar at a time. This should allow the test to run quite a number of times before we need to manually create 
			// a new order for the test to continue functioning.

			using (var client = CreateTestClient())
			{
				var createRefundRequest = new RefundOrderRequest() { MerchantRefundReference = System.Guid.NewGuid().ToString(), OrderId = Environment.GetEnvironmentVariable("ZipPayments_TestOrderIdForRefunds"), Amount = 1, Operator = "Test", TerminalId = "2531" };
				var refundResponse = await client.RefundOrderAsync(createRefundRequest);
				Assert.IsNotNull(refundResponse);
				Assert.IsFalse(String.IsNullOrEmpty(refundResponse.Id));
				Assert.IsNotNull(refundResponse.RefundedDateTime);
				Assert.AreEqual(createRefundRequest.MerchantRefundReference, refundResponse.MerchantReference);
				Assert.AreEqual(createRefundRequest.Amount, refundResponse.Amount);
			}
		}

		[TestMethod]
		public async Task ZipClient_RefundOrder_ThrowsZipApiExceptionOnErrorResponse()
		{
			using (var client = CreateTestClient())
			{
				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => 
					{
						var request = new RefundOrderRequest() { MerchantRefundReference = System.Guid.NewGuid().ToString(), OrderId = System.Guid.NewGuid().ToString(), Amount = 1, Operator = "Test", TerminalId = "2531" };
						var refundResponse = await client.RefundOrderAsync(request); 
					}
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_CanCommitAuthedOrder()
		{
			using (var client = CreateTestClient())
			{

				#region Create Authed Order

				var createOrderRequest = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA00",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Auth
					}
				};

				var createOrderResponse = await client.CreateOrderAsync(createOrderRequest);
				Assert.IsNotNull(createOrderResponse);
				Assert.IsFalse(String.IsNullOrWhiteSpace(createOrderResponse.OrderId));

				#endregion

				await client.CommitOrderAsync(new CommitOrderRequest() { OrderId = createOrderResponse.OrderId });

				var statusResponse = await client.GetOrderStatusAsync(new OrderStatusRequest() { OrderId = createOrderResponse.OrderId });
				Assert.IsNotNull(statusResponse);
				Assert.AreEqual(ZipOrderStatus.Complete, statusResponse.Status);
			}
		}

		[TestMethod]
		public async Task ZipClient_CommitOrder_ThrowsApiExceptionForErrorResponse()
		{
			using (var client = CreateTestClient())
			{
				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.CommitOrderAsync(new CommitOrderRequest() { OrderId = System.Guid.NewGuid().ToString() }); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_CanRollbackAuthedOrder()
		{
			using (var client = CreateTestClient())
			{

				#region Create Authed Order

				var createOrderRequest = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA00",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Auth
					}
				};

				var createOrderResponse = await client.CreateOrderAsync(createOrderRequest);
				Assert.IsNotNull(createOrderResponse);
				Assert.IsFalse(String.IsNullOrWhiteSpace(createOrderResponse.OrderId));

				#endregion

				await client.RollbackOrderAsync(new RollbackOrderRequest() { OrderId = createOrderResponse.OrderId });

				var statusResponse = await client.GetOrderStatusAsync(new OrderStatusRequest() { OrderId = createOrderResponse.OrderId });
				Assert.IsNotNull(statusResponse);
				Assert.AreEqual(ZipOrderStatus.Cancelled, statusResponse.Status);
			}
		}

		[TestMethod]
		public async Task ZipClient_RollbackOrder_ThrowsApiExceptionForErrorResponse()
		{
			using (var client = CreateTestClient())
			{
				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.RollbackOrderAsync(new RollbackOrderRequest() { OrderId = System.Guid.NewGuid().ToString() }); }
				);
			}
		}

		[Ignore("Requires you to manually specify the authorisation code and secret.")]
		[TestMethod]
		public async Task ZipClient_EnrolTest()
		{
			using (var client = CreateTestClient())
			{

				var enrolRequest = new EnrolRequest()
				{
					ActivationCode = "ABC",
					Secret = "123",
					Terminal = "2531"
				};

				var enrolResponse = await client.EnrolAsync(enrolRequest);

				Assert.IsNotNull(enrolResponse);
			}
		}

		[TestMethod]
		public async Task ZipClient_Enrol_ThrowsZipApiExceptionOnErrorResponse()
		{
			using (var client = CreateTestClient())
			{

				var enrolRequest = new EnrolRequest()
				{
					ActivationCode = "ABC",
					Secret = "123",
					Terminal = "2531"
				};

				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.EnrolAsync(enrolRequest); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_ThrowsUnauthorisedAccessException_WhenUnableToRetrieveToken()
		{
			using (var client = new ZipClient
			(
				new ZipClientConfiguration
				(
					"Invalid",
					"Invalid",
					ZipEnvironment.NewZealand.Test
				)
			))
			{

				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment
					}
				};

				await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>
				(
					async () => { await client.CreateOrderAsync(request); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_ThrowsUnauthorisedAccessException_WhenNoClientAuthDetails()
		{
			using (var client = new ZipClient
			(
				new ZipClientConfiguration
				(
					null,
					null,
					ZipEnvironment.NewZealand.Test
				)
			))
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment
					}
				};

				await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>
				(
					async () => { await client.CreateOrderAsync(request); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_ThrowsZipApiException_ForBadRequest()
		{
			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AM00",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment
					}
				};

				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.CreateOrderAsync(request); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_ThrowsZipApiException_ForNotFoundResponse()
		{
			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "Z001",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment
					}
				};

				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { await client.CreateOrderAsync(request); }
				);
			}
		}

		[TestMethod]
		public async Task ZipClient_IncludesHttpStatusCode_ForZipApiException()
		{
			using (var client = CreateTestClient())
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "Z001",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment
					}
				};

				try
				{
					await client.CreateOrderAsync(request);
					throw new InvalidOperationException("Expected ZipApiException was not thrown.");
				}
				catch(ZipApiException ex)
				{
					Assert.IsTrue(ex.Errors.ResponseCode == 404);
				}
			}
		}

		[TestMethod]
		public async Task ZipClient_CallsRequestApplyDefaults()
		{
			using (var client = CreateTestClient("2531", "Kermit The Frog", "Albany"))
			{
				var request = new CreateOrderRequest()
				{
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = "AA05",
						MerchantReference = System.Guid.NewGuid().ToString(),
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
					}
				};

				try
				{
					var result = await client.CreateOrderAsync(request);
				}
				catch { } //We don't care about actual errors

				//Check the defaults were applied after we attempted the order creation
				Assert.AreEqual("2531", request.TerminalId);
				Assert.AreEqual("Kermit The Frog", request.Order.Operator);
				Assert.AreEqual("Albany", request.StoreId);
			}
		}

		[TestMethod]
		public async Task ZipClient_CallsRequestValidate()
		{
			using (var client = CreateTestClient())
			{
				//Create a request known to fail validation
				//(several required properties including pre-approval code are null)
				var request = new CreateOrderRequest()
				{
					Order = new ZipOrder()
					{
						Amount = 10.5M,
						CustomerApprovalCode = null,
						MerchantReference = System.Guid.NewGuid().ToString(),
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 10.50M,
							Quantity = 1,
							Sku = "123"
						}
					}
					}
				};

				await Assert.ThrowsExceptionAsync<ArgumentNullException>
				(
					async () => { await client.CreateOrderAsync(request); }
				);
			}
		}

		private IZipClient CreateTestClient()
		{
			return CreateTestClient(null, null, null);
		}

		private IZipClient CreateTestClient(string terminalId, string operatorId, string storeId)
		{
			return new ZipClient
			(
				new ZipClientConfiguration
				(
					Environment.GetEnvironmentVariable("ZipPayments_ClientId"),
					Environment.GetEnvironmentVariable("ZipPayments_ClientSecret"),
					ZipEnvironment.NewZealand.Test
				)
				{
					DefaultTerminalId = terminalId,
					DefaultOperator = operatorId,
					DefaultStoreId = storeId
				}
			);
		}

	}
}
