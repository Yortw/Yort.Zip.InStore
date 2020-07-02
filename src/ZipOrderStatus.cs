using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Provides a list of known statuses for Zip orders.
	/// </summary>
	public static class ZipOrderStatus
	{
		/// <summary>
		/// Indicates the order is still pending approval or cancellation by the system/consumer. The client should continue polling.
		/// </summary>
		public const string Pending = "pending";

		/// <summary>
		/// The order has been approved and paid for by the customer. This status is relevant to an order that has been created with the <see cref="ZipPaymentFlow.Auth"/> flow.
		/// </summary>
		public const string Approved = "approved";

		/// <summary>
		/// The order has been paid and the order is complete. This means either the <see cref="ZipPaymentFlow.Payment"/> flow as used and payment successful, or the the <see cref="ZipPaymentFlow.Auth"/> was used and the payment was committed.
		/// </summary>
		public const string Complete = "complete";

		/// <summary>
		/// The order was declined by the user.
		/// </summary>
		public const string Declined = "declined";

		/// <summary>
		/// The order has timed out.
		/// </summary>
		public const string Expired = "expired";

		/// <summary>
		/// The order has been cancelled either prior to the customer approving/declining, or the order has been rolled back by the merchant after a succesful <see cref="ZipPaymentFlow.Auth"/>.
		/// </summary>
		public const string Cancelled = "cancelled";

		/// <summary>
		/// Returns true if the status provider is a known, final status - that is a status that should not change again.
		/// </summary>
		/// <param name="status">The status to check.</param>
		/// <returns>True if the status is final, otherwise false.</returns>
		public static bool IsTerminalStatus(string status)
		{
			return status == Complete
				|| status == Declined
				|| status == Expired
				|| status == Cancelled;
		}
	}
}
