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
	}
}
