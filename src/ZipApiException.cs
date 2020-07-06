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
		public ZipApiException() : this(ErrorMessage.UnknownApiError) { }
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
		/// <param name="error">A <see cref="ZipErrorResponse"/> containing full errors received from the Zip API.</param>
		public ZipApiException(ZipErrorResponse error) : base(ErrorMessageFromZipErrors(error)) { Errors = error; }
		/// <summary>
		/// Full constructor, recommended.
		/// </summary>
		/// <param name="error">A <see cref="ZipErrorResponse"/> containing full errors received from the Zip API.</param>
		/// <param name="inner">Another exception being wrapped by this one.</param>
		public ZipApiException(ZipErrorResponse error, Exception inner) : base(ErrorMessageFromZipErrors(error), inner) { Errors = error; }

		/// <summary>
		/// Returns a <see cref="ZipErrorResponse"/> instance containing details of the full set of errors, or null if no response content was provided by the Zip API.
		/// </summary>
		public ZipErrorResponse? Errors
		{
			get
			{
				if (this.Data.Contains("ZipErrors"))
					return this.Data["ZipErrors"] as ZipErrorResponse;

				return null;
			}
			private set
			{
				this.Data["ZipErrors"] = value;
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

		private static string ErrorMessageFromZipErrors(ZipErrorResponse error)
		{
			//Often (not always) a '400' response has a pretty generic 'error.Message' value which isn't helpful (i.e 'The request is invalid'),
			//in this case include the 'error.ErrorCode' value which often provides a better hint about what is wrong ('AboveMaximumPreApprovalAmount').
			if (error.ResponseCode == 400 && !String.IsNullOrEmpty(error.ErrorCode) && !String.IsNullOrEmpty(error.Message))
				return $"({error.ErrorCode}) {error.Message}";

			return error?.Message
				?? error?.Detail 
				?? error?.ValidationErrors?.FirstOrDefault().ErrorMessages?.FirstOrDefault()
				?? error?.ErrorCode
				?? error?.Title
				?? ErrorMessage.UnknownApiError;
		}
	}
}
