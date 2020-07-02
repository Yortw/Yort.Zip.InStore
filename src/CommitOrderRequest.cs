using System;
using System.Collections.Generic;
using System.Text;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Contains details of a request to commit an order that has been created using the <see cref="ZipPaymentFlow.Auth"/> flow.
	/// </summary>
	public class CommitOrderRequest : ZipRequestOptionsBase
	{
		/// <summary>
		/// The unique id of the order within the Zip system, as returned in <see cref="CreateOrderResponse.OrderId"/>.
		/// </summary>
		public string OrderId { get; set; } = null!;

		/// <summary>
		/// Validates this request as much as possible prior to sending it to the Zip API.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Thrown if any required properties are null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any of request properties are invalid.</exception>
		public override void Validate()
		{
			OrderId.GuardNullOrWhiteSpace(nameof(OrderId));
		}
	}
}
