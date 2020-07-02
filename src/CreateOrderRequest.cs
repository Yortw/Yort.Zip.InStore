using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Contains details of an order (request for payment) to be created within the Zip system.
	/// </summary>
	/// <seealso cref="CreateOrderRequest"/>
	/// <seealso cref="IZipClient.CreateOrderAsync(CreateOrderRequest)"/>
	public class CreateOrderRequest : ZipRequestOptionsBase
	{
		/// <summary>
		/// Determines if Zip is instructed to check if the <see cref="ZipOrder.MerchantReference"/> has already been used and if so behave in an idempotent fashion (return the original response instead of creating a duplicate order).
		/// </summary>
		/// <remarks>
		/// <para>The default value is true, which enables the duplicate checking and idempotent responses. Set to false only if you want to allow duplicate orders with the same merchant reference to be created.</para>
		/// <para>If this is set to false and a merchant reference is reused, a new order with a new Zip order id will be created.</para>
		/// </remarks>
		public bool EnableUniqueMerchantReferenceCheck { get; set; } = true;

		/// <summary>
		/// Optional. When using a merchant level authentication scope this is supposed to give traceability from which store the order originated in.
		/// </summary>
		public string StoreId { get; set; } = null!;

		/// <summary>
		/// Required. A value that uniquley identifies the point of sale terminal being used as part of this request.
		/// </summary>
		public string TerminalId { get; set; } = null!;
		 
		/// <summary>
		/// Specifies the details of the new Zip order to be created.
		/// </summary>
		public ZipOrder Order { get; set; } = null!;

		/// <summary>
		/// Validates this request as much as possible prior to sending it to the Zip API.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Thrown if <see cref="Order"/> or any of it's required sub-properties are null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any of the sub-properties of <see cref="Order"/> are determined to be invalid.</exception>
		public override void Validate()
		{
			TerminalId.GuardNullOrWhiteSpace(nameof(TerminalId));
			Order.GuardNull(nameof(Order));
			Order.CustomerApprovalCode.GuardNullOrWhiteSpace(nameof(Order), nameof(Order.CustomerApprovalCode));
			Order.MerchantReference.GuardNullOrWhiteSpace(nameof(Order), nameof(Order.MerchantReference));
			Order.Operator.GuardNullOrWhiteSpace(nameof(Order), nameof(Order.Operator));
			Order.PaymentFlow.GuardNullOrWhiteSpace(nameof(Order), nameof(Order.PaymentFlow));
			Order.Amount.GuardZeroOrNegative(nameof(Order), nameof(Order.Amount));
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

		/// <summary>
		/// Applies the default store id, terminal id and operator if appropriate.
		/// </summary>
		/// <param name="config">A <see cref="ZipClientConfiguration"/> instance containing the default values to use.</param>
		public override void ApplyDefaults(ZipClientConfiguration config)
		{
			if (config == null) return;
			if (this.Order == null) return;

			this.Order.Operator = this.Order.Operator ?? config.DefaultOperator;
			this.StoreId ??= config.DefaultStoreId;
			this.TerminalId ??= config.DefaultTerminalId;

			base.ApplyDefaults(config);
		}
	}
}
