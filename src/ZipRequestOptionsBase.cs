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

		/// <summary>
		/// Applies default values from the <see cref="ZipClientConfiguration"/> specified to this request, where appropriate.
		/// </summary>
		/// <param name="config">A <see cref="ZipClientConfiguration"/> to read default values from.</param>
		/// <remarks>
		/// <para>Defaults should only be used for values that are not otherwise set (null).</para>
		/// <para>The base version of this method is a no-op, it must be overridden by derived classes to implement the defaults specific to their request format.</para>
		/// <para>This method is called (by <see cref="ZipClient"/>) before the <see cref="Validate"/> method, and should be robust to null/missing/out of range values.</para>
		/// </remarks>
		public virtual void ApplyDefaults(ZipClientConfiguration config) { }
	}
}
