using System;
using System.Collections.Generic;
using System.Text;
using Ladon;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Represents a request to allow a device to request the client id and secret required to request authorisation tokens.
	/// </summary>
	public class EnrolRequest : ZipRequestOptionsBase
	{
		/// <summary>
		/// The activation code generated in the Zip merchant portal. Required.
		/// </summary>
		public string ActivationCode { get; set; } = null!;
		/// <summary>
		/// The secret used when generating the activation code in the Zip merchant portal. Required.
		/// </summary>
		public string Secret { get; set; } = null!;
		/// <summary>
		/// The name of the terminal being enroled. Required.
		/// </summary>
		public string Terminal { get; set; } = null!;

		/// <summary>
		/// Validates this request as much as possible prior to sending it to the Zip API.
		/// </summary>
		/// <exception cref="System.ArgumentNullException">Thrown if any required properties are null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any of request properties are invalid.</exception>
		public override void Validate()
		{
			this.ActivationCode.GuardNullOrWhiteSpace(nameof(ActivationCode));
			this.Secret.GuardNullOrWhiteSpace(nameof(Secret));
			this.Terminal.GuardNullOrWhiteSpace(nameof(Terminal));
		}

		/// <summary>
		/// Applies the default store id, terminal id and operator if appropriate.
		/// </summary>
		/// <param name="config">A <see cref="ZipClientConfiguration"/> instance containing the default values to use.</param>
		public override void ApplyDefaults(ZipClientConfiguration config)
		{
			if (config == null) return;

			this.Terminal ??= config.DefaultTerminalId;

			base.ApplyDefaults(config);
		}
	}
}
