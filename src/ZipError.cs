using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// Used to hold error messages and validation errors returned by the Zip API.
	/// </summary>
	[Serializable]
	public class ZipErrorResponse 
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public ZipErrorResponse() { }
		/// <summary>
		/// The HTTP status code returned with the response that contained these errors.
		/// </summary>
		public int ResponseCode { get; set; }
		/// <summary>
		/// Sets or returns the top level error message associated with the response.
		/// </summary>
		public string? Message { get; set; }

		/// <summary>
		/// Sets or returns a generic description of the error message.
		/// </summary>
		public string? Title { get; set; }

		/// <summary>
		/// Sets or returns an error message specific to this instance of the error (as opposed to <see cref="Title"/>).
		/// </summary>
		public string? Detail { get; set; }

		/// <summary>
		/// Sets or returns a unique value that can be reported to Zip to assist with locating log entries relating to this request.
		/// </summary>
		public string? TraceId { get; set; }

		/// <summary>
		/// Sets or returns a string containing an 'error code' that can be used to programmatically take action on a specific error.
		/// </summary>
		public string? ErrorCode { get; set; }

		/// <summary>
		/// An array of errors, keyed by the name of the property that as an error and with the value being an array of related error messages.
		/// </summary>
		[JsonPropertyName("errors")]
		public IEnumerable<ZipValidationError>? ValidationErrors { get; set; }

		/// <summary>
		/// Returns a boolean indicatin whether or not the request was valid.
		/// </summary>
		public bool IsValid { get; set; }

		/// <summary>
		/// A string containing a uri that defines the specific type of error that occurred.
		/// </summary>
		public string? Type { get; set; }

	}

	/// <summary>
	/// Represents a set of errors that relate to a specific request property.
	/// </summary>
	[Serializable]
	public class ZipValidationError
	{
		/// <summary>
		/// The name of the property these errors relate to.
		/// </summary>
		public string? PropertyName { get; set; }
		/// <summary>
		/// A set of error messages stating problems with the property specified.
		/// </summary>
		public IEnumerable<string>? ErrorMessages { get; set; }
	}
}
