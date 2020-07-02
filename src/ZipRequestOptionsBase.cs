using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Base class for request options classes, providing a common and extensible interface for all requests.
	/// </summary>
	public abstract class ZipRequestOptionsBase
	{

		/// <summary>
		/// Validates the properties of this request match the known design time requirements, i.e are all required properties provided.
		/// </summary>
		/// <remarks>
		/// <para>Throws exceptions (typically <see cref="ArgumentException"/>, <see cref="ArgumentNullException"/> or <see cref="ArgumentOutOfRangeException"/>) if any property is in a known invalid state.</para>
		/// </remarks>
		public abstract void Validate();

		/// <summary>
		/// Returns a list of key value pairs to be applied as HTTP headers to HTTP request for this request type.
		/// </summary>
		/// <remarks>
		/// <para>By default returns null indicating there are no custom headers. Can be overridden by derived types to specify otherwise.</para>
		/// </remarks>
		/// <returns>A dictionary containing key/value pairs to be applied as custom HTTP headers, or null if no custom headers apply.</returns>
		public virtual Dictionary<string, string>? GetCustomHttpHeaders()
		{
			return null;
		}
	}
}
