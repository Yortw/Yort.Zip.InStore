using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Contains details of a successful request to cancel an order via <see cref="IZipClient.CancelOrderAsync(CancelOrderRequest)"/>.
	/// </summary>
	public class CancelOrderResponse
	{
		/// <summary>
		/// Contains the unique id of the order that was cancelled.
		/// </summary>
		public string OrderId { get; set; } = null!;
	}
}
