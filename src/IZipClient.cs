using System;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// An interface for the primary object used to access the Zip payments API.
	/// </summary>
	/// <remarks>
	/// <para>This interface exists primarily to support mocking/stubbing out of the api client for testing purposes. Ideally client code should use this interface as the type for all reference to the implementation instance to support runtime replacement of the implementation.</para>
	/// </remarks>
	public interface IZipClient
	{
	}
}
