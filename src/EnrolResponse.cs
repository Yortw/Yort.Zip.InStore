﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Contains the result of a successful <see cref="EnrolRequest"/>.
	/// </summary>
	public class EnrolResponse
	{
		/// <summary>
		/// The client id to be used when requesting new authentication tokens from Zip.
		/// </summary>
		/// <remarks>
		/// <para>NOTE: The 'enrollment system' is untested as the test account provided to the library authors did not support generating activation codes. This should work, but if it doesn't, please log an issue.</para>
		/// </remarks>
		[JsonPropertyName("client_id")]
		public string ClientId { get; set; } = null!;
		/// <summary>
		/// The client secret to be used when requesting new authentication tokens from Zip.
		/// </summary>
		[JsonPropertyName("client_secret")]
		public string ClientSecret { get; set; } = null!;
	}
}
