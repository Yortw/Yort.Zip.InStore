using System;
using System.Linq;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// An exception thrown by <see cref="ZipClient"/> when an error response is received from the Zip API.
	/// </summary>
	[Serializable]
	public class ZipApiException : Exception
	{
		/// <summary>
		/// Default constructor. Not recommended for use.
		/// </summary>
		public ZipApiException() { }
		/// <summary>
		///Partial constructor. Not recommended for use.
		/// </summary>
		/// <param name="message">The error message of the exception.</param>
		public ZipApiException(string message) : base(message) { }
		/// <summary>
		/// Partial constructor. Not recommended for use.
		/// </summary>
		/// <param name="message">The error message for this exception.</param>
		/// <param name="inner">Another exception being wrapped by this one.</param>
		public ZipApiException(string message, Exception inner) : base(message, inner) { }
		/// <summary>
		/// Partial constructor, recommended.
		/// </summary>
		/// <param name="error">A <see cref="ZipError"/> containing full errors received from the Zip API.</param>
		public ZipApiException(ZipError error) : base(ErrorMessageFromZipErrors(error)) { Errors = error; }
		/// <summary>
		/// Full constructor, recommended.
		/// </summary>
		/// <param name="error">A <see cref="ZipError"/> containing full errors received from the Zip API.</param>
		/// <param name="inner">Another exception being wrapped by this one.</param>
		public ZipApiException(ZipError error, Exception inner) : base(ErrorMessageFromZipErrors(error), inner) { Errors = error; }

		/// <summary>
		/// Returns a <see cref="ZipError"/> instance containing details of the full set of errors, or null if no response content was provided by the Zip API.
		/// </summary>
		public ZipError? Errors
		{
			get
			{
				if (this.Data.Contains("ZipErrors"))
					return this.Data["ZipErrors"] as ZipError;

				return null;
			}
			private set
			{
				this.Data["Errors"] = value;
			}
		}

		/// <summary>
		/// Deserialisation constructor.
		/// </summary>
		/// <param name="info">The serialisation information to deserialise from.</param>
		/// <param name="context">The streaming context to deserialise from.</param>
		protected ZipApiException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

		private static string ErrorMessageFromZipErrors(ZipError error)
		{
			return error?.Message ?? error?.Errors?.FirstOrDefault().Value?.FirstOrDefault() ?? error?.ErrorCode ?? error?.Title ?? ErrorMessage.UnknownApiError;
		}
	}
}
