using System;
using System.Collections.Generic;
using System.Text;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Provides details to a <see cref="ZipClient"/> instance about how to access the Zip API.
	/// </summary>
	public class ZipClientConfiguration
	{
		private readonly string? _ClientId;
		private readonly string? _ClientSecret;
		private readonly ZipEnvironment _Environment;

		/// <summary>
		/// Full constructor.
		/// </summary>
		/// <param name="clientId">The client id used to request new authentication tokens. Can be null if you plan to retrieve it using the <see cref="IZipClient.EnrolAsync(EnrolRequest)"/> mechanism.</param>
		/// <param name="clientSecret">The client secret used to request new authentication tokens. Can be null if you plan to retrieve it using the <see cref="IZipClient.EnrolAsync(EnrolRequest)"/> mechanism.</param>
		/// <param name="environment">The environment indicating the API instance to access. Required.</param>
		public ZipClientConfiguration(string? clientId, string? clientSecret, ZipEnvironment environment)
		{
			_ClientId = clientId;
			_ClientSecret = clientSecret;
			_Environment = environment.GuardNull(nameof(environment));
		}

		/// <summary>
		/// Returns the client id value provided via the constructor.
		/// </summary>
		public string? ClientId => _ClientId;

		/// <summary>
		/// Returns the client secret value provided via the constructor.
		/// </summary>
		public string? ClientSecret => _ClientSecret;

		/// <summary>
		/// Returns the <see cref="ZipEnvironment"/> value provided via the constructor.
		/// </summary>
		public ZipEnvironment Environment => _Environment;

		/// <summary>
		/// Provides the default store id to be sent on requests (that require a store id) if the request value is not already set (null).
		/// </summary>
		public string DefaultStoreId { get; set; } = null!;

		/// <summary>
		/// Provides the default terminal id to be sent on requests (that require a terminal id) if the request value is not already set (null).
		/// </summary>
		public string DefaultTerminalId { get; set; } = null!;

		/// <summary>
		/// Provides the default (POS) operator value to be sent on requests (that require an operator value) if the request value is not already set (null).
		/// </summary>
		public string DefaultOperator { get; set; } = null!;
	}
}
