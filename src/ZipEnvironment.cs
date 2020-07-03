using System;
using System.Collections.Generic;
using System.Text;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Represents a particular environment for the Zip API (i.e test for NZ merchants, production for AU merchants etc).
	/// </summary>
	/// <remarks>
	/// <para>You can construct instances of this class in order to support future environments that may come online before the library is updated with support, but for convenience use the pre-built instances provided 
	/// by the static properties on this class, i.e <see cref="ZipEnvironment.NewZealand.Test"/> provides the test environment for the NZ region.</para>
	/// </remarks>
	/// <seealso cref="ZipEnvironment.NewZealand.Production"/>
	/// <seealso cref="ZipEnvironment.NewZealand.Test"/>
	public class ZipEnvironment
	{
		private readonly string _Audience;
		private readonly Uri _TokenEndpoint;
		private readonly Uri _BaseUrl;

		/// <summary>
		/// Full contructor.
		/// </summary>
		/// <param name="audience">The 'audience' or 'API Identifier' required when requesting an auth token for this environment.</param>
		/// <param name="tokenEndpoint">The full URL used to request an auth token to access the API for this endpoint.</param>
		/// <param name="baseUrl">The root url (including common sub-path) to the API for this environment.</param>
		public ZipEnvironment(string audience, Uri tokenEndpoint, Uri baseUrl)
		{
			_Audience = audience.GuardNullOrWhiteSpace(nameof(audience));
			_TokenEndpoint = tokenEndpoint.GuardNull(nameof(tokenEndpoint));
			_BaseUrl = baseUrl.GuardNull(nameof(baseUrl));
		}

		/// <summary>
		/// Returns the 'audience'/'API Identifier' associated with this environment.
		/// </summary>
		public string Audience { get { return _Audience; } }

		/// <summary>
		/// Returns the full url used to retrieve an auth token for this environment.
		/// </summary>
		public Uri TokenEndpoint { get { return _TokenEndpoint; } }

		/// <summary>
		/// Returns base url (including common root path) for this environment.
		/// </summary>
		public Uri BaseUrl { get { return _BaseUrl; } }

		private static readonly ZipEnvironment s_NewZealandProduction = new ZipEnvironment
		(
			"https://auth.partpay.co.nz",
			new Uri("https://merchant-auth-nz.zip.co/oauth/token"),
			new Uri("https://zip.co/nz/api/")
		);

		private static readonly ZipEnvironment s_NewZealandTest = new ZipEnvironment
		(
			"https://auth-dev.partpay.co.nz",
			new Uri("https://partpay-dev.au.auth0.com/oauth/token"),
			new Uri("https://api-ci.partpay.co.nz/")
		);

		/// <summary>
		/// Provides pre-build environment instances for the New Zealand region.
		/// </summary>
		public static class NewZealand
		{
			/// <summary>
			/// Provides a pre-configured <see cref="ZipEnvironment"/> instance with correct values for the test environment for NZ merchants.
			/// </summary>
			public static ZipEnvironment Test { get { return s_NewZealandTest; } }

			/// <summary>
			/// Provides a pre-configured <see cref="ZipEnvironment"/> instance with correct values for the production environment for NZ merchants.
			/// </summary>
			public static ZipEnvironment Production { get { return s_NewZealandProduction; } }

		}
	}
}
