﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// The main class used to interact with the Zip payments API.
	/// </summary>
	/// <seealso cref="IZipClient"/>
	public class ZipClient : IZipClient
	{

		private readonly ZipEnvironment _Environment;
		private readonly HttpClient _HttpClient;
		private bool _IsDisposed;

		private readonly System.Text.Json.JsonSerializerOptions _SerializerOptions = new System.Text.Json.JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			AllowTrailingCommas = true,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		};

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="environment">The <see cref="ZipEnvironment"/> for the API to call.</param>
		public ZipClient(ZipEnvironment environment)
		{
			_Environment = environment.GuardNull(nameof(environment));
			_HttpClient = new HttpClient()
			{
				BaseAddress = _Environment.BaseUrl
			};
		}

		/// <summary>
		/// Creates a new order (payment) with Zip.
		/// </summary>
		/// <param name="request">Details of the order to be created.</param>
		/// <returns>Details of the created order if succesful, otherwise throws an exception.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
		public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
		{
			using (var response = await PostJsonAsync("pos/order", request, request.Order).ConfigureAwait(false))
			{
				//Zip API docs says 'Created' in the text description for this response, but API actually returns 'Accepted'... we'll support both just in case (both are technically 'success responses').
				if (response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.Found)
					return await JsonResponseToEntityAsync<CreateOrderResponse>(response).ConfigureAwait(false);

				throw ZipApiExceptionFromResponse(response);
			}
		}

		/// <summary>
		/// Requests the current status of the specified order from the Zip API.
		/// </summary>
		/// <param name="request">A <see cref="OrderStatusRequest"/> providing details of the order to return the status of.</param>
		/// <returns>If successful a <see cref="OrderStatusResponse"/> containing details of the specified order's status. Otherwise throws an exception.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		public async Task<OrderStatusResponse> GetOrderStatusAsync(OrderStatusRequest request)
		{
			using (var response = await GetJsonAsync($"v2.0/pos/order/{request.OrderId}/status", request).ConfigureAwait(false))
			{
				//Zip API docs says 'Created' in the text description for this response, but API actually returns 'Accepted'... we'll support both just in case (both are technically 'success responses').
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
					return await JsonResponseToEntityAsync<OrderStatusResponse>(response).ConfigureAwait(false);

				throw ZipApiExceptionFromResponse(response);
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

		private async Task EnsureAuthTokenAsync()
		{
			using (var client = new HttpClient())
			{
				var request = new ClientCredentialsRequest()
				{
					client_id = "q5cga0ptk97tp3mTNzEIO9GlyWFvbnlA",
					client_secret = "cOhW9puYS-IasBRkkAtJ42k7exK0_OV7jae8JlpyISlnbUo6aWvzmgVPuBhmNI6k",
					audience = _Environment.Audience,
					grant_type = "client_credentials"
				};

				using (var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.UTF8Encoding.UTF8, "application/json"))
				{
					var response = await client.PostAsync(_Environment.TokenEndpoint, content).ConfigureAwait(false);
					response.EnsureSuccessStatusCode();

					var tokenDetails = System.Text.Json.JsonSerializer.Deserialize<ClientCredentialsResponse>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

					_HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(tokenDetails.token_type, tokenDetails.access_token);
				}
			}
		}

		private HttpContent CreateJsonContent<T>(T body)
		{
			return new StringContent(System.Text.Json.JsonSerializer.Serialize<T>(body, _SerializerOptions), System.Text.UTF8Encoding.UTF8, "application/json");
		}

		private async Task<HttpResponseMessage> PostJsonAsync<T, TRequestOptions>(string relativePath, TRequestOptions request, T requestEntity) where TRequestOptions : ZipRequestOptionsBase
		{
			EnsureNotDisposed();

			request.GuardNull(nameof(request));
			request.Validate();

			await EnsureAuthTokenAsync().ConfigureAwait(false);

			using (var content = CreateJsonContent(requestEntity))
			{
				return await _HttpClient.PostAsync(new Uri(relativePath, UriKind.Relative), content).ConfigureAwait(false);
			}
		}

		private async Task<HttpResponseMessage> GetJsonAsync(string relativePath, OrderStatusRequest request)
		{
			EnsureNotDisposed();

			request.GuardNull(nameof(request));
			request.Validate();

			await EnsureAuthTokenAsync().ConfigureAwait(false);

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

		private Exception ZipApiExceptionFromResponse(HttpResponseMessage response)
		{
			//TODO: This
			throw new NotImplementedException();
		}


		private class ClientCredentialsRequest
		{
			public string? client_id { get; set; }
			public string? client_secret { get; set; }
			public string? audience { get; set; }
			public string? grant_type { get; set; }
		}

		private class ClientCredentialsResponse
		{
			public string? access_token { get; set; }
			public long expires_in { get; set; }
			public string? scope { get; set; }
			public string? token_type { get; set; }
		}
	}
}
