using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ladon;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// The main class used to interact with the Zip payments API.
	/// </summary>
	/// <seealso cref="IZipClient"/>
	public class ZipClient : IZipClient
	{

		private readonly HttpClient _HttpClient;
		private readonly ZipClientConfiguration _Configuration;

		private bool _IsDisposed;
		private AuthToken? _AuthToken;

		private readonly System.Text.Json.JsonSerializerOptions _SerializerOptions = new System.Text.Json.JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			AllowTrailingCommas = true,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		};

		/// <summary>
		/// Partial constructor.
		/// </summary>
		/// <param name="configuration">A <see cref="ZipClientConfiguration"/> instance providing client credentials, the API environment to access and other required details.</param>
		public ZipClient(ZipClientConfiguration configuration) : this(null, configuration)
		{
		}

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <remarks>
		/// <para>If you are injecting an instance of <see cref="HttpClient"/> via the <paramref name="httpClient"/> make sure the 'AllowAutoRedirect' property on the inner most handler (and any intermediate ones) 
		/// is set to false. The Zip API returns redirect responses in some situations that must be handled manually by the <see cref="ZipClient"/> instance for correct behaviour to be applied.</para>
		/// </remarks>
		/// <param name="httpClient">See the method remarks for important information. An <see cref="HttpClient"/> instance to use to access the Zip API, or null to have the ZipClient create it's own internally. Supply your own if you wish to apply handlers for logging, retry logic etc.</param>
		/// <param name="configuration">A <see cref="ZipClientConfiguration"/> instance providing client credentials, the API environment to access and other required details.</param>
		public ZipClient(HttpClient? httpClient, ZipClientConfiguration configuration)
		{
			_Configuration = configuration.GuardNull(nameof(configuration));
			_HttpClient = httpClient ?? CreateNewHttpClient();
			_HttpClient.BaseAddress = _Configuration.Environment.BaseUrl;
		}

		/// <summary>
		/// Creates a new order (payment) with Zip.
		/// </summary>
		/// <param name="request">Details of the order to be created.</param>
		/// <remarks>
		/// <para>If the <see cref="CreateOrderRequest.EnableUniqueMerchantReferenceCheck"/> is true and the specified <see cref="ZipOrder.MerchantReference"/> has 
		/// been used before this method will behave in an idempotent way, returning a successful response but without creating a duplicate order. If 
		/// <see cref="CreateOrderRequest.EnableUniqueMerchantReferenceCheck"/> is false and duplicate merchant reference is used, a dupliate order will be created in Zip.
		/// </para>
		/// </remarks>
		/// <returns>Details of the created order if succesful, otherwise throws an exception.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Thrown for some types of network and HTTP failures, usually where the request has not made it to the server for processing.</exception>
		/// <exception cref="OperationCanceledException">Thrown if a timeout occurs calling the Zip API.</exception>
		public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
		{	
			using (var response = await PostJsonAsync("pos/order", request, request.Order).ConfigureAwait(false))
			{
				//Zip API docs says 'Created' in the text description for this response, but API actually returns 'Accepted'... we'll support both just in case (both are technically 'success responses').
				if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.Created)
					return await JsonResponseToEntityAsync<CreateOrderResponse>(response).ConfigureAwait(false);
				else if (response.StatusCode == System.Net.HttpStatusCode.Found)
				{
					//The API is documented as returning a response body for this status the same as for the accepted/created statuses, 
					//but it doesn't actually seem to. We have to be able to get the order id in order to be able to perform refunds
					//in the future, so just following the redirect to the status endpoint which doesn't give us the order id isn't useful.
					//After discussing with Zip it's been decided (for now) to retrieve the order id from the location url (ugh).
					//At that point, for the context of this library, we should just pretend like we got that normally and let the client 
					//deal with only one behaviour from this method instead of following the redirect.
					if (response.Content == null || response.Content.Headers.ContentLength == 0)
					{
						var retVal = CreateOrderResponseFromUrl(response.Headers.Location);
						if (retVal != null) return retVal;

						throw new ZipApiException(ErrorMessage.UnexpectedCreateOrderRedirectResponse);
					}
					else
						return await JsonResponseToEntityAsync<CreateOrderResponse>(response).ConfigureAwait(false);
				}

				throw await ZipApiExceptionFromResponseAsync(response).ConfigureAwait(false);
			}
		}

		private CreateOrderResponse? CreateOrderResponseFromUrl(Uri location)
		{
			//Sample url format
			//https://api-ci.partpay.co.nz/v2.0/pos/order/1b5bca35-9fb5-461a-aa92-0854a7265890/status

			if (location == null) return null;
			var urlStr = location.ToString();
			if (!urlStr.EndsWith("/status", StringComparison.OrdinalIgnoreCase)) return null;

			var separatorIndex = urlStr.LastIndexOf("/", urlStr.Length - 8, StringComparison.Ordinal);

			return new CreateOrderResponse()
			{
				OrderId = urlStr.Substring(separatorIndex + 1, urlStr.Length - (8 + separatorIndex))
			};
		}

		/// <summary>
		/// Requests a previously created order be cancelled.
		/// </summary>
		/// <param name="request">A <see cref="CancelOrderRequest"/> providing details of the order to be cancelled and operation/terminal that is requesting cancellation.</param>
		/// <returns>A <see cref="CancelOrderResponse"/> instance.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Thrown for some types of network and HTTP failures, usually where the request has not made it to the server for processing.</exception>
		/// <exception cref="OperationCanceledException">Thrown if a timeout occurs calling the Zip API.</exception>
		public async Task<CancelOrderResponse> CancelOrderAsync(CancelOrderRequest request)
		{
			using (var response = await PostJsonAsync("pos/order/cancel", request, new { cancelOrderId = request.OrderId, @operator = request.Operator }).ConfigureAwait(false))
			{
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
					return await JsonResponseToEntityAsync<CancelOrderResponse>(response).ConfigureAwait(false);

				throw await ZipApiExceptionFromResponseAsync(response).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Requests the current status of the specified order from the Zip API.
		/// </summary>
		/// <param name="request">A <see cref="OrderStatusRequest"/> providing details of the order to return the status of.</param>
		/// <returns>If successful a <see cref="OrderStatusResponse"/> containing details of the specified order's status. Otherwise throws an exception.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Thrown for some types of network and HTTP failures, usually where the request has not made it to the server for processing.</exception>
		/// <exception cref="OperationCanceledException">Thrown if a timeout occurs calling the Zip API.</exception>
		public async Task<OrderStatusResponse> GetOrderStatusAsync(OrderStatusRequest request)
		{
			using (var response = await GetJsonAsync($"v2.0/pos/order/{request.OrderId}/status", request).ConfigureAwait(false))
			{
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
					return await JsonResponseToEntityAsync<OrderStatusResponse>(response).ConfigureAwait(false);

				throw await ZipApiExceptionFromResponseAsync(response).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Refunds some or all of the money taken as part of a previously completed order.
		/// </summary>
		/// <param name="request">A <see cref="RefundOrderRequest"/> specifying details of the refund to create and the order to create it against.</param>
		/// <returns>A <see cref="RefundOrderResponse"/> with details of a succesful outcome.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Thrown for some types of network and HTTP failures, usually where the request has not made it to the server for processing.</exception>
		/// <exception cref="OperationCanceledException">Thrown if a timeout occurs calling the Zip API.</exception>
		public async Task<RefundOrderResponse> RefundOrderAsync(RefundOrderRequest request)
		{
			var requestBodyEntity = new
			{
				@operator = request.Operator,
				amount = request.Amount,
				merchantRefundReference = request.MerchantRefundReference
			};

			using (var response = await PostJsonAsync($"pos/order/{request.OrderId}/refund", request, requestBodyEntity).ConfigureAwait(false))
			{
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
					return await JsonResponseToEntityAsync<RefundOrderResponse>(response).ConfigureAwait(false);

				throw await ZipApiExceptionFromResponseAsync(response).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Commits (completes) an order previously created via <see cref="CreateOrderAsync(CreateOrderRequest)"/> using the <see cref="ZipPaymentFlow.Auth"/> flow.
		/// </summary>
		/// <param name="request">A <see cref="CommitOrderRequest"/> containing details of the order to commit.</param>
		/// <returns>A task that can be awaited to know when the operation has completed. If the task does not return an exception, the auth completed succesfully. Use the <see cref="GetOrderStatusAsync(OrderStatusRequest)"/> to confirm.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Thrown for some types of network and HTTP failures, usually where the request has not made it to the server for processing.</exception>
		/// <exception cref="OperationCanceledException">Thrown if a timeout occurs calling the Zip API.</exception>
		public async Task CommitOrderAsync(CommitOrderRequest request)
		{
			using (var response = await PostJsonAsync($"pos/order/{request.OrderId}/commit", request, (object)null!).ConfigureAwait(false))
			{
				//Zip API docs says 'Ok' in the text description for this response, but API actually returns 'Accepted'... we'll support both just in case (both are technically 'success responses').
				if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted)
					return;

				throw await ZipApiExceptionFromResponseAsync(response).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Rolls back (cancels/undoes) an order previously created via <see cref="CreateOrderAsync(CreateOrderRequest)"/> using the <see cref="ZipPaymentFlow.Auth"/> flow.
		/// </summary>
		/// <param name="request">A <see cref="RollbackOrderRequest"/> containing details of the order to rollback.</param>
		/// <returns>A task that can be awaited to know when the operation has completed. If the task does not return an exception, the auth rolled back succesfully. Use the <see cref="GetOrderStatusAsync(OrderStatusRequest)"/> to confirm.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Thrown for some types of network and HTTP failures, usually where the request has not made it to the server for processing.</exception>
		/// <exception cref="OperationCanceledException">Thrown if a timeout occurs calling the Zip API.</exception>
		public async Task RollbackOrderAsync(RollbackOrderRequest request)
		{
			using (var response = await PostJsonAsync($"pos/order/{request.OrderId}/rollback", request, (object)null!).ConfigureAwait(false))
			{
				//Zip API docs says 'Ok' in the text description for this response, but API actually returns 'Accepted'... we'll support both just in case (both are technically 'success responses').
				if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted)
					return;

				throw await ZipApiExceptionFromResponseAsync(response).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Allows retrieval of the client id and secret used to request new auth tokens using the Zip device enrolment system.
		/// </summary>
		/// <remarks>
		/// <para>On a successful response, not only does the <see cref="EnrolResponse"/> returned contain the client id and secret so the caller can persist them 
		/// for future sessions, but the client id and secret returned will be automatically applied to the configuration for this <see cref="ZipClient"/> instance.
		/// </para>
		/// </remarks>
		/// <param name="request">A <see cref="EnrolRequest"/> instance providing details of the device to enrol.</param>
		/// <returns>A <see cref="EnrolResponse"/> instance containing details of the token returned.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		/// <exception cref="System.Net.Http.HttpRequestException">Thrown for some types of network and HTTP failures, usually where the request has not made it to the server for processing.</exception>
		/// <exception cref="OperationCanceledException">Thrown if a timeout occurs calling the Zip API.</exception>
		public async Task<EnrolResponse> EnrolAsync(EnrolRequest request)
		{
			using (var response = await PostJsonAsync("pos/terminal/enrol", request, request).ConfigureAwait(false))
			{
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
				{
					var retVal = await JsonResponseToEntityAsync<EnrolResponse>(response).ConfigureAwait(false);

					_Configuration.ClientId = retVal.ClientId;
					_Configuration.ClientSecret = retVal.ClientSecret;

					return retVal;
				}

				throw await ZipApiExceptionFromResponseAsync(response).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Disposes this instance and all internal resources.
		/// </summary>
		/// <seealso cref="Dispose(bool)"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "We do call suppress finalise, but it's not recognised because of the try/finally.")]
		public void Dispose()
		{
			try
			{
				_IsDisposed = true;
				Dispose(true);
			}
			finally
			{
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// Disposes resources used by this instance.
		/// </summary>
		/// <param name="disposing">True if dispose is being called explicitly by client code, or false if it is being called from a finalizer (indicating only unmanaged resources should be cleaned up).</param>
		/// <seealso cref="Dispose()"/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				_HttpClient?.Dispose();
		}

		private void EnsureNotDisposed()
		{
			if (_IsDisposed) throw new ObjectDisposedException(nameof(ZipClient));
		}

		private async Task EnsureAuthTokenValidAsync()
		{
			if (!(_AuthToken?.IsExpired() ?? true)) return;

			if (String.IsNullOrEmpty(_Configuration.ClientId) || String.IsNullOrEmpty(_Configuration.ClientSecret))
				throw new UnauthorizedAccessException(ErrorMessage.ClientAuthDetailsRequired);

			using (var client = CreateNewHttpClient())
			{
				var request = new AuthTokenRequest()
				{
					client_id = _Configuration.ClientId,
					client_secret = _Configuration.ClientSecret,
					audience = _Configuration.Environment.Audience,
					grant_type = "client_credentials"
				};

				using (var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.UTF8Encoding.UTF8, "application/json"))
				{
					var response = await client.PostAsync(_Configuration.Environment.TokenEndpoint, content).ConfigureAwait(false);
					if (response.StatusCode != System.Net.HttpStatusCode.OK)
						throw new UnauthorizedAccessException($"({Convert.ToInt32(response.StatusCode, System.Globalization.CultureInfo.InvariantCulture)}) " + ErrorMessage.UnableToRetrieveAuthToken);
					response.EnsureSuccessStatusCode();

					var newToken = System.Text.Json.JsonSerializer.Deserialize<AuthToken>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

					_HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(newToken.token_type, newToken.access_token);

					_AuthToken = newToken;
				}
			}
		}

		private HttpContent CreateJsonContent<T>(T body)
		{
			if (body == null) return new StringContent(String.Empty, System.Text.UTF8Encoding.UTF8, "application/json");

			return new StringContent(System.Text.Json.JsonSerializer.Serialize<T>(body, _SerializerOptions), System.Text.UTF8Encoding.UTF8, "application/json");
		}

		private async Task<HttpResponseMessage> PostJsonAsync<T, TRequestOptions>(string relativePath, TRequestOptions request, T requestEntity) where TRequestOptions : ZipRequestOptionsBase
		{
			EnsureNotDisposed();

			request.GuardNull(nameof(request));
			request.ApplyDefaults(_Configuration);
			request.Validate();

			await EnsureAuthTokenValidAsync().ConfigureAwait(false);

			using (var content = CreateJsonContent(requestEntity))
			{
				using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(_HttpClient.BaseAddress, relativePath)) { Content = content })
				{
					requestMessage.Headers.Authorization = _HttpClient.DefaultRequestHeaders.Authorization;

					ApplyCustomHttpHeadersForRequest(request, requestMessage);
					
					return await _HttpClient.SendAsync(requestMessage).ConfigureAwait(false);
				}
			}
		}

		private static void ApplyCustomHttpHeadersForRequest<TRequestOptions>(TRequestOptions request, HttpRequestMessage requestMessage) where TRequestOptions : ZipRequestOptionsBase
		{
			var customHeaders = request.GetCustomHttpHeaders();
			if (customHeaders != null)
			{
				foreach (var headerKvp in customHeaders)
				{
					if (!requestMessage.Headers.TryAddWithoutValidation(headerKvp.Key, headerKvp.Value))
						throw new InvalidOperationException($"Failed to add custom http header \"{headerKvp.Key}\".");
				}
			}
		}

		private async Task<HttpResponseMessage> GetJsonAsync(string relativePath, OrderStatusRequest request)
		{
			EnsureNotDisposed();

			request.GuardNull(nameof(request));
			request.ApplyDefaults(_Configuration);
			request.Validate();

			await EnsureAuthTokenValidAsync().ConfigureAwait(false);

			return await _HttpClient.GetAsync(new Uri(relativePath, UriKind.Relative)).ConfigureAwait(false);
		}

		private async Task<T> JsonResponseToEntityAsync<T>(HttpResponseMessage response)
		{
			return System.Text.Json.JsonSerializer.Deserialize<T>
			(
				await response.Content.ReadAsStringAsync().ConfigureAwait(false),
				_SerializerOptions
			);
		}

		private async Task<Exception> ZipApiExceptionFromResponseAsync(HttpResponseMessage response)
		{
			ZipErrorResponse errors = null!;
			//If we fail to parse the error body that's unfortunate, but don't hide the original error.
			//TODO: should perhaps log or otherwise expose the deserialisation failure without hiding the 
			//original exception. For now, just do our best to report the exception that really matters (original).
			string? content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

			if (String.IsNullOrEmpty(content))
			{
				errors = new ZipErrorResponse()
				{
					Message = response.ReasonPhrase,
					ResponseCode = Convert.ToInt32(response.StatusCode, System.Globalization.CultureInfo.InvariantCulture)
				};
			}
			else
			{
				try
				{
					errors = System.Text.Json.JsonSerializer.Deserialize<ZipErrorResponse>(content, _SerializerOptions);
				}
				catch { }
			}

			errors.ResponseCode = Convert.ToInt32(response.StatusCode, System.Globalization.CultureInfo.InvariantCulture);

			throw new ZipApiException(errors);
		}

		private static HttpClient CreateNewHttpClient()
		{
			var handler = new HttpClientHandler();
			try
			{
				if (handler.SupportsAutomaticDecompression)
					handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;

				if (handler.SupportsRedirectConfiguration)
					handler.AllowAutoRedirect = false;
				
				return new HttpClient(handler);
			}
			catch
			{
				handler?.Dispose();
				throw;
			}
		}

	}
}
