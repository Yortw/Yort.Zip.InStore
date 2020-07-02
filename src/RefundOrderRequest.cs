using System;
using System.Collections.Generic;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Represents a request to refund all or part of a previously completed order.
	/// </summary>
	public class RefundOrderRequest : ZipRequestOptionsBase
	{
		/// <summary>
		/// A unique reference for this refund. Required.
		/// </summary>
		public string MerchantRefundReference { get; set; } = null!;
		/// <summary>
		/// The unique reference of the order within the Zip system to be refunded against. Required.
		/// </summary>
		public string OrderId { get; set; } = null!;
		/// <summary>
		/// The amount of this refund. Required, must be a positive and non-zero value.
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// A reference (name or id) of the staff person serving the customer at the POS. Required.
		/// </summary>
		public string Operator { get; set; } = null!;

		/// <summary>
		/// Required. A value that uniquley identifies the point of sale terminal being used as part of this request. 
		/// </summary>
		public string TerminalId { get; set; } = null!;

		/// <summary>
		/// Optional. When using a merchant level authentication scope this is supposed to give traceability from which store the order originated in.
		/// </summary>
		public string StoreId { get; set; } = null!;

		/// <summary>
		/// Validates this request as much as possible prior to sending it to the Zip API.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Thrown if any required properties are null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any of request properties are invalid.</exception>
		public override void Validate()
		{
			MerchantRefundReference.GuardNullOrWhiteSpace(nameof(MerchantRefundReference));
			OrderId.GuardNullOrWhiteSpace(nameof(OrderId));
			Amount.GuardZeroOrNegative(nameof(Amount));
			Operator.GuardNullOrWhiteSpace(nameof(Operator));
			TerminalId.GuardNullOrWhiteSpace(nameof(TerminalId));
		}

		/// <summary>
		/// Returns custom header values for <see cref="StoreId"/> (if not null or empty) and <see cref="TerminalId"/>.
		/// </summary>
		/// <returns></returns>
		public override Dictionary<string, string>? GetCustomHttpHeaders()
		{
			var retVal = new Dictionary<string, string>
			{
				{ "partpay-terminalid", this.TerminalId }
			};

			if (!String.IsNullOrEmpty(this.StoreId))
				retVal.Add("store-id", this.StoreId);

			return retVal;
		}
	}
}
