using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Contains the initial details of an order created in the Zip system.
	/// </summary>
	/// <seealso cref="NewZipOrder"/>
	/// <seealso cref="IZipClient.CreateOrderAsync(CreateOrderRequest)"/>
	public class CreateOrderResponse
	{
		/// <summary>
		/// Gets or sets the unique id of the newly created order (or existing order using the same merchant reference, see <see cref="CreateOrderRequest.EnableUniqueMerchantReferenceCheck"/>) within the Zip system.
		/// </summary>
		public string OrderId { get; set; } = null!;

		/// <summary>
		/// Gets or sets the expiry time of the order as returned by the Zip API.
		/// </summary>
		public DateTimeOffset? OrderExpiry { get; set; } = null;
	}
}
