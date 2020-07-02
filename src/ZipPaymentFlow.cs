using System;
using System.Collections.Generic;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// A class that exposes constants for the known allowed values of the <see cref="NewZipOrder.PaymentFlow"/> property.
	/// </summary>
	public static class ZipPaymentFlow
	{
		/// <summary>
		/// Indicates that payment should be fully completed without the requirement to 'capture' the outcome.
		/// </summary>
		public const string Payment = "payment";
		/// <summary>
		/// Indicates that an auth should be taken by the initial order creation, and a subsequent call to commit or rollback the payment should be made.
		/// </summary>
		public const string Auth = "auth";
	}
}
