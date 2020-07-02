using System;
using System.Collections.Generic;
using System.Text;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Represents details of a request to retrieve the current status of an order in the Zip system.
	/// </summary>
	public class OrderStatusRequest : ZipRequestOptionsBase
	{
		/// <summary>
		/// The unique id of the order within the Zip system, as returned in <see cref="CreateOrderResponse.OrderId"/>.
		/// </summary>
		public string OrderId { get; set; } = null!;

		/// <summary>
		/// Validates this request as much as possible prior to sending it to the Zip API.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Thrown if <see cref="OrderId"/> is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if <see cref="OrderId"/> is blank or contains only whitespace.</exception>
		public override void Validate()
		{
			OrderId.GuardNullOrWhiteSpace(nameof(OrderId));
		}
	}
}
