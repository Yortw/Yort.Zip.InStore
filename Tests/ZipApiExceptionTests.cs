using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yort.Zip.InStore.Tests
{
	[TestClass]
	public class ZipApiExceptionTests
	{

		[TestMethod]
		public void ZipApiException_UsesExplicitErrorMessage()
		{
			var ex = new ZipApiException("Test message");
			Assert.AreEqual("Test message", ex.Message);
		}

		[TestMethod]
		public void ZipApiException_UsesExplicitErrorMessageAndInnerException()
		{
			var inner = new HttpRequestException();
			var ex = new ZipApiException("Test message", inner);
			Assert.AreEqual("Test message", ex.Message);
			Assert.AreEqual(inner, ex.InnerException);
		}

		[TestMethod]
		public void ZipApiException_WithErrorResponse_UsesErrorMessage()
		{
			var errorResponse = new ZipErrorResponse()
			{
				ErrorCode = "TestErrorCode",
				IsValid = false,
				Message = "Operator is required",
				ValidationErrors = new ZipValidationError[]
				{
					new ZipValidationError()
					{
						PropertyName = "Operator",
						ErrorMessages = new string[]
						{
							"Operator is required"
						}
					}
				}
			};

			var ex = new ZipApiException(errorResponse);
			Assert.AreEqual("Operator is required", ex.Message);
			Assert.IsNotNull(ex.Errors);
		}

		[TestMethod]
		public void ZipError_CanDeserialiseFromJsonResponse()
		{
			var json = @"{""message"":""'Operator' must not be empty."",""isValid"":false,""errors"":[{""propertyName"":""operator"",""errorMessages"":[""'Operator' must not be empty.""]}],""type"":""https://partpay.net/errors/property-validation""}";

			var error = System.Text.Json.JsonSerializer.Deserialize<ZipErrorResponse>(json, new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
			Assert.IsNotNull(error);
			Assert.AreEqual("'Operator' must not be empty.", error.Message);
			Assert.AreEqual(false, error.IsValid);
			Assert.AreEqual(1, error.ValidationErrors.Count());
			Assert.AreEqual("operator", error.ValidationErrors.FirstOrDefault().PropertyName);
			Assert.AreEqual(1, error.ValidationErrors.FirstOrDefault().ErrorMessages.Count());
			Assert.AreEqual("'Operator' must not be empty.", error.ValidationErrors.FirstOrDefault().ErrorMessages.FirstOrDefault());

			json = @"{""message"":""'Amount' must not be empty."",""isValid"":false,""errors"":[{""propertyName"":""amount"",""errorMessages"":[""'Amount' must not be empty."",""'Amount' must be greater than '0'.""]},{""propertyName"":""operator"",""errorMessages"":[""'Operator' must not be empty.""]},{""propertyName"":""merchantReference"",""errorMessages"":[""'Merchant Reference' must not be empty.""]},{""propertyName"":""customerApprovalCode"",""errorMessages"":[""'Customer Approval Code' must not be empty.""]}],""type"":""https://partpay.net/errors/property-validation""}";

			error = System.Text.Json.JsonSerializer.Deserialize<ZipErrorResponse>(json, new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
			Assert.IsNotNull(error);
			Assert.AreEqual("'Amount' must not be empty.", error.Message);
			Assert.AreEqual(false, error.IsValid);
			Assert.AreEqual("https://partpay.net/errors/property-validation", error.Type);

			var validationErrors = error.ValidationErrors.ToArray();
			Assert.AreEqual(4, validationErrors.Count());

			Assert.AreEqual("amount", validationErrors[0].PropertyName);
			var errorMessages = validationErrors[0].ErrorMessages.ToArray();
			Assert.AreEqual(2, errorMessages.Count());
			Assert.AreEqual("'Amount' must not be empty.", errorMessages[0]);
			Assert.AreEqual("'Amount' must be greater than '0'.", errorMessages[1]);

			Assert.AreEqual("operator", validationErrors[1].PropertyName);
			errorMessages = validationErrors[1].ErrorMessages.ToArray();
			Assert.AreEqual(1, errorMessages.Count());
			Assert.AreEqual("'Operator' must not be empty.", errorMessages[0]);

			Assert.AreEqual("merchantReference", validationErrors[2].PropertyName);
			errorMessages = validationErrors[2].ErrorMessages.ToArray();
			Assert.AreEqual(1, errorMessages.Count());
			Assert.AreEqual("'Merchant Reference' must not be empty.", errorMessages[0]);

			Assert.AreEqual("customerApprovalCode", validationErrors[3].PropertyName);
			errorMessages = validationErrors[3].ErrorMessages.ToArray();
			Assert.AreEqual(1, errorMessages.Count());
			Assert.AreEqual("'Customer Approval Code' must not be empty.", errorMessages[0]);
		}

	}
}
