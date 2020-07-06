using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yort.Zip.InStore.Tests
{
	internal class MockHttpHandler : DelegatingHandler
	{
		private readonly Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>> _MockResponses;

		public MockHttpHandler()
		{
			_MockResponses = new Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>>();
		}

		public void AddMock(HttpMethod method, string fullUrl, Func<HttpRequestMessage, Task<HttpResponseMessage>> responseGenerator)
		{
			_MockResponses[method + ":" + fullUrl] = responseGenerator;
		}

		protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var key = request.Method.ToString() + ":" + request.RequestUri.ToString();

			if (_MockResponses.TryGetValue(key, out var response))
				return await response(request);

			return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
		}

	}
}
