using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Represents a request to cancel an order that has not yet been approved by the consumer or reached a final status.
	/// </summary>
	public class CancelOrderRequest : ZipRequestOptionsBase
	{
		/// <summary>
		/// The unique id of the order within the Zip system, as returned in <see cref="CreateOrderResponse.OrderId"/>.
		/// </summary>
		public string OrderId { get; set; } = null!;

		/// <summary>
		/// A reference (name or id) of the staff person serving the customer at the POS. Required.
		/// </summary>
		public string Operator { get; set; } = null!;

		/// <summary>
		/// Required. A value that uniquley identifies the point of sale terminal being used as part of this request.
		/// </summary>
		public string TerminalId { get; set; } = null!;

		/// <summary>
		/// Validates this request as much as possible prior to sending it to the Zip API.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Thrown if any required properties are null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any of request properties are invalid.</exception>
		public override void Validate()
		{
			TerminalId.GuardNullOrWhiteSpace(nameof(this.TerminalId));
			OrderId.GuardNullOrWhiteSpace(nameof(this.OrderId));
			Operator.GuardNullOrWhiteSpace(nameof(this.Operator));
		}
	}
}
