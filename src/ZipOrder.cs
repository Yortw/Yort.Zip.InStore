﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Contains details of an order (request for payment) to be created within the Zip system.
	/// </summary>
	/// <seealso cref="CreateOrderResponse"/>
	/// <seealso cref="IZipClient.CreateOrderAsync(CreateOrderRequest)"/>
	public class ZipOrder
	{
		/// <summary>
		/// The pre-approval code generated by the csutomer using their Zip app or web login. Required.
		/// </summary>
		public string? CustomerApprovalCode { get; set; } = null!;
		/// <summary>
		/// A reference (name or id) of the staff person serving the customer at the POS. Required.
		/// </summary>
		public string? Operator { get; set; } = null!;
		/// <summary>
		/// The amount to be charged to the consumer by Zip. Required. Must be a positive, non-zero value.
		/// </summary>
		public decimal Amount { get; set; }
		/// <summary>
		/// The unique reference for this order as generated by the merchant. Required.
		/// </summary>
		public string? MerchantReference { get; set; } = null!;

		/// <summary>
		/// Determines whether this order results in a payment or an authorisation which must be subsequently confirmed. Required.
		/// </summary>
		/// <remarks>
		/// <para>The default is <see cref="ZipPaymentFlow.Payment"/>.</para>
		/// </remarks>
		/// <seealso cref="ZipPaymentFlow"/>
		public string? PaymentFlow { get; set; } = ZipPaymentFlow.Payment;

		/// <summary>
		/// A summary of the items purchased on this order which will be displayed to the end consumer as part of their purchase history. Optional.
		/// </summary>
		/// <remarks>
		/// <para>This collection defaults to null, to add items first set it to either an empty collection or a pre-loaded collection of <see cref="ZipOrderItem"/> instances.</para>
		/// </remarks>
		public List<ZipOrderItem> Items { get; set; } = null!;

	}
}