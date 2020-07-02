using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Represents the response of a successful <see cref="IZipClient.GetOrderStatusAsync(OrderStatusRequest)"/> call.
	/// </summary>
	public class OrderStatusResponse
	{
		/// <summary>
		/// The human readable form of a Zip order id.
		/// </summary>
		public string OrderNumber { get; set; } = null!;

		/// <summary>
		/// The status of the order.
		/// </summary>
		public string Status { get; set; } = null!;
	}
}
