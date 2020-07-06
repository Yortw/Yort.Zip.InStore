using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Contains the initial details of an order created in the Zip system.
	/// </summary>
	/// <seealso cref="ZipOrder"/>
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
		/// <remarks>
		/// <para>Note this property can be null, and will be in the event of a 'duplicate check' occuring where the response only returns the order id.</para>
		/// </remarks>
		public DateTimeOffset? OrderExpiry { get; set; } = null;
	}
}
