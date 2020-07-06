using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yort.Zip.InStore.Tests
{
	[TestClass]
	public class ZipClientUnitTests
	{

		private static readonly System.Text.Json.JsonSerializerOptions _SerialiserOptions = new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

		#region CreateOrder Duplicate Request Response Tests

		/// <summary>
		/// This test checks the system handles/responds appropriately the *documented* response for the create order end point when it is called multiple times with the same client reference 
		/// and the duplicate checks are enabled. Note that at the time of writing the documented behaviour is not the actual behaviour, but this test *should* ensure that if the API changes to 
		/// match the documentation then we will still work as expected.
		/// </summary>
		/// <returns></returns>
		[TestMethod]
		public async Task CreateOrder_HandlesDuplicateCheckResponse_WithDocumentedBody()
		{

			#region Setup Mocked Client

			var responseHistory = new Dictionary<string, CreateOrderResponse>();

			var handler = new MockHttpHandler();

			handler.AddMock
			(
				HttpMethod.Post, "https://api-ci.partpay.co.nz/pos/order",
				async (httpRequest) =>
				{
					HttpStatusCode responseCode = HttpStatusCode.Created;
					var order = System.Text.Json.JsonSerializer.Deserialize<ZipOrder>(await httpRequest.Content.ReadAsStringAsync(), _SerialiserOptions);

					var key = order.MerchantReference;
					if (!responseHistory.TryGetValue(key, out var responseBody))
					{
						responseBody = responseHistory[key] = new CreateOrderResponse()
						{
							OrderId = System.Guid.NewGuid().ToString(),
							OrderExpiry = DateTime.UtcNow.AddHours(1)
						};
					}
					else
					{
						responseCode = HttpStatusCode.Found;
						responseBody = new CreateOrderResponse() { OrderId = responseBody.OrderId };
					}

					var retVal = new HttpResponseMessage(responseCode)
					{
						Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(responseBody, _SerialiserOptions), System.Text.UTF8Encoding.UTF8, "application/json")
					};

					if (responseCode == HttpStatusCode.Found)
					{
						retVal.Headers.Location = new Uri(httpRequest.RequestUri, $"/v2.0/pos/order/{responseBody.OrderId}/status");
					}

					return retVal;
				}
			);

			var httpClient = new HttpClient(handler);

			#endregion

			using (var client = CreateTestClient(httpClient))
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					EnableUniqueMerchantReferenceCheck = true,
					Order = new ZipOrder()
					{
						Amount = 50M,
						CustomerApprovalCode = "206931",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 50M,
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

				Assert.AreEqual(result1.OrderId, result2.OrderId);
			}
		}

		/// <summary>
		/// This test checks the system handles/responds appropriately the actual response seen for the create order end point when it is called multiple times with the same client reference 
		/// and the duplicate checks are enabled. Note that at the time of writing the documented behaviour is not the actual behaviour, but this test checks we handle the actual behaviour which is a 
		/// 302 found response with no body and a location header set. We can retrieve the critical 'orderid' value by parsing the location header url.
		/// </summary>
		/// <returns></returns>
		[TestMethod]
		public async Task CreateOrder_HandlesDuplicateCheckResponse_WithOnlyLocationHeader()
		{

			#region Setup Mocked Client

			var responseHistory = new Dictionary<string, CreateOrderResponse>();

			var handler = new MockHttpHandler();

			handler.AddMock
			(
				HttpMethod.Post, "https://api-ci.partpay.co.nz/pos/order",
				async (httpRequest) =>
				{
					HttpStatusCode responseCode = HttpStatusCode.Created;
					var order = System.Text.Json.JsonSerializer.Deserialize<ZipOrder>(await httpRequest.Content.ReadAsStringAsync(), _SerialiserOptions);

					var key = order.MerchantReference;
					if (!responseHistory.TryGetValue(key, out var responseBody))
					{
						responseBody = responseHistory[key] = new CreateOrderResponse()
						{
							OrderId = System.Guid.NewGuid().ToString(),
							OrderExpiry = DateTime.UtcNow.AddHours(1)
						};
					}
					else
					{
						responseCode = HttpStatusCode.Found;
					}

					var retVal = new HttpResponseMessage(responseCode)
					{
						Content = responseCode == HttpStatusCode.Found ? null : new StringContent(System.Text.Json.JsonSerializer.Serialize(responseBody, _SerialiserOptions), System.Text.UTF8Encoding.UTF8, "application/json")
					};

					if (responseCode == HttpStatusCode.Found)
					{
						retVal.Headers.Location = new Uri(httpRequest.RequestUri, $"/v2.0/pos/order/{responseBody.OrderId}/status");
					}

					return retVal;
				}
			);

			var httpClient = new HttpClient(handler);

			#endregion

			using (var client = CreateTestClient(httpClient))
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					EnableUniqueMerchantReferenceCheck = true,
					Order = new ZipOrder()
					{
						Amount = 50M,
						CustomerApprovalCode = "206931",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 50M,
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

				Assert.AreEqual(result1.OrderId, result2.OrderId);
			}
		}

		/// <summary>
		/// This test confirms we throw an exception if we get a duplicate check response where we can't figure out an order id
		/// </summary>
		/// <returns></returns>
		[TestMethod]
		public async Task CreateOrder_ThrowsOnUnexpectedDuplicateResponse()
		{

			#region Setup Mocked Client

			var responseHistory = new Dictionary<string, CreateOrderResponse>();

			var handler = new MockHttpHandler();

			handler.AddMock
			(
				HttpMethod.Post, "https://api-ci.partpay.co.nz/pos/order",
				async (httpRequest) =>
				{
					HttpStatusCode responseCode = HttpStatusCode.Created;
					var order = System.Text.Json.JsonSerializer.Deserialize<ZipOrder>(await httpRequest.Content.ReadAsStringAsync(), _SerialiserOptions);

					var key = order.MerchantReference;
					if (!responseHistory.TryGetValue(key, out var responseBody))
					{
						responseBody = responseHistory[key] = new CreateOrderResponse()
						{
							OrderId = System.Guid.NewGuid().ToString(),
							OrderExpiry = DateTime.UtcNow.AddHours(1)
						};
					}
					else
					{
						responseCode = HttpStatusCode.Found;
					}

					var retVal = new HttpResponseMessage(responseCode)
					{
						Content = responseCode == HttpStatusCode.Found ? null : new StringContent(System.Text.Json.JsonSerializer.Serialize(responseBody, _SerialiserOptions), System.Text.UTF8Encoding.UTF8, "application/json")
					};

					if (responseCode == HttpStatusCode.Found)
					{
						retVal.Headers.Location = new Uri(httpRequest.RequestUri, $"/v2.0/pos/order/{responseBody.OrderId}/nobodyexpectsthespanishinquisition");
					}

					return retVal;
				}
			);

			var httpClient = new HttpClient(handler);

			#endregion

			using (var client = CreateTestClient(httpClient))
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					EnableUniqueMerchantReferenceCheck = true,
					Order = new ZipOrder()
					{
						Amount = 50M,
						CustomerApprovalCode = "206931",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 50M,
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

				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { var result2 = await client.CreateOrderAsync(request); }
				);
			}
		}

		/// <summary>
		/// This test confirms we throw an exception if we get a duplicate check response where we can't figure out an order id
		/// </summary>
		/// <returns></returns>
		[TestMethod]
		public async Task CreateOrder_ThrowsOnRedirectResponseWithNoLocation()
		{

			#region Setup Mocked Client

			var responseHistory = new Dictionary<string, CreateOrderResponse>();

			var handler = new MockHttpHandler();

			handler.AddMock
			(
				HttpMethod.Post, "https://api-ci.partpay.co.nz/pos/order",
				async (httpRequest) =>
				{
					HttpStatusCode responseCode = HttpStatusCode.Created;
					var order = System.Text.Json.JsonSerializer.Deserialize<ZipOrder>(await httpRequest.Content.ReadAsStringAsync(), _SerialiserOptions);

					var key = order.MerchantReference;
					if (!responseHistory.TryGetValue(key, out var responseBody))
					{
						responseBody = responseHistory[key] = new CreateOrderResponse()
						{
							OrderId = System.Guid.NewGuid().ToString(),
							OrderExpiry = DateTime.UtcNow.AddHours(1)
						};
					}
					else
					{
						responseCode = HttpStatusCode.Found;
					}

					var retVal = new HttpResponseMessage(responseCode)
					{
						Content = responseCode == HttpStatusCode.Found ? null : new StringContent(System.Text.Json.JsonSerializer.Serialize(responseBody, _SerialiserOptions), System.Text.UTF8Encoding.UTF8, "application/json")
					};

					return retVal;
				}
			);

			var httpClient = new HttpClient(handler);

			#endregion

			using (var client = CreateTestClient(httpClient))
			{
				var request = new CreateOrderRequest()
				{
					TerminalId = "2531",
					EnableUniqueMerchantReferenceCheck = true,
					Order = new ZipOrder()
					{
						Amount = 50M,
						CustomerApprovalCode = "206931",
						MerchantReference = System.Guid.NewGuid().ToString(),
						Operator = "Test",
						PaymentFlow = ZipPaymentFlow.Payment,
						Items = new System.Collections.Generic.List<ZipOrderItem>()
					{
						new ZipOrderItem()
						{
							Name = "Test Item",
							Description = "0110A Blue 12",
							Price = 50M,
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

				await Assert.ThrowsExceptionAsync<ZipApiException>
				(
					async () => { var result2 = await client.CreateOrderAsync(request); }
				);
			}
		}

		#endregion

		#region Enrol Tests

		/// <summary>
		/// As we're not currently able to test the enrol end points actual behaviour, this unit test uses a mock to check that we handled the documented 
		/// responses appropriately. Hopefully this means we work in reality.
		/// </summary>
		/// <returns></returns>
		[TestMethod]
		public async Task Enrol_Handles_SuccessfulResponse()
		{

			#region Setup Mocked Client

			var responseHistory = new Dictionary<string, CreateOrderResponse>();

			var handler = new MockHttpHandler();

			handler.AddMock
			(
				HttpMethod.Post, "https://api-ci.partpay.co.nz/pos/terminal/enrol",
				(httpRequest) =>
				{
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = new StringContent
						(
							System.Text.Json.JsonSerializer.Serialize
							(
								new
								{
									client_id = "3D888947-1CA3-428B-A428-F53532FB8593",
									client_secret = "6FFD61D3-76CA-4059-8AE2-C246B52FF452"
								},
								_SerialiserOptions
							),
							System.Text.UTF8Encoding.UTF8,
							"application/json"
						)
					});
				}
			);

			var httpClient = new HttpClient(handler);

			#endregion

			using (var client = CreateTestClient(httpClient))
			{
				var request = new EnrolRequest()
				{
					ActivationCode = "ABC123",
					Secret = "zipme!",
					Terminal = "2531"
				};

				var result = await client.EnrolAsync(request);
				Assert.IsNotNull(result);
				Assert.AreEqual("3D888947-1CA3-428B-A428-F53532FB8593", result.ClientId);
				Assert.AreEqual("6FFD61D3-76CA-4059-8AE2-C246B52FF452", result.ClientSecret);
			}
		}

		#endregion

		private IZipClient CreateTestClient(HttpClient httpClient)
		{
			return new ZipClient
			(
				httpClient,
				new ZipClientConfiguration
				(
					Environment.GetEnvironmentVariable("ZipPayments_ClientId"),
					Environment.GetEnvironmentVariable("ZipPayments_ClientSecret"),
					ZipEnvironment.NewZealand.Test
				)
			);
		}

	}
}
