using System;
using System.Threading.Tasks;

namespace Yort.Zip.InStore
{
	/// <summary>
	/// An interface for the primary object used to access the Zip payments API.
	/// </summary>
	/// <remarks>
	/// <para>This interface exists primarily to support mocking/stubbing out of the api client for testing purposes. Ideally client code should use this interface as the type for all reference to the implementation instance to support runtime replacement of the implementation.</para>
	/// </remarks>
	/// <seealso cref="ZipClient"/>
	public interface IZipClient : IDisposable
	{
		/// <summary>
		/// Creates a new order (payment) with Zip.
		/// </summary>
		/// <param name="request">Details of the order to be created.</param>
		/// <returns>Details of the created order if succesful, otherwise throws an exception.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);

		/// <summary>
		/// Requests the current status of the specified order from the Zip API.
		/// </summary>
		/// <param name="request">A <see cref="OrderStatusRequest"/> providing details of the order to return the status of.</param>
		/// <returns>If successful a <see cref="OrderStatusResponse"/> containing details of the specified order's status. Otherwise throws an exception.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		Task<OrderStatusResponse> GetOrderStatusAsync(OrderStatusRequest request);

		/// <summary>
		/// Requests a previously created order be cancelled.
		/// </summary>
		/// <param name="request">A <see cref="CancelOrderRequest"/> providing details of the order to be cancelled and operation/terminal that is requesting cancellation.</param>
		/// <returns>A <see cref="CancelOrderResponse"/> instance.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		Task<CancelOrderResponse> CancelOrderAsync(CancelOrderRequest request);

		/// <summary>
		/// Refunds some or all of the money taken as part of a previously completed order.
		/// </summary>
		/// <param name="request">A <see cref="RefundOrderRequest"/> specifying details of the refund to create and the order to create it against.</param>
		/// <returns>A <see cref="RefundOrderResponse"/> with details of a succesful outcome.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		Task<RefundOrderResponse> RefundOrderAsync(RefundOrderRequest request);

		/// <summary>
		/// Commits (completes) an order previously created via <see cref="CreateOrderAsync(CreateOrderRequest)"/> using the <see cref="ZipPaymentFlow.Auth"/> flow.
		/// </summary>
		/// <param name="request">A <see cref="CommitOrderRequest"/> containing details of the order to commit.</param>
		/// <returns>A task that can be awaited to know when the operation has completed. If the task does not return an exception, the auth completed succesfully. Use the <see cref="GetOrderStatusAsync(OrderStatusRequest)"/> to confirm.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		Task CommitOrderAsync(CommitOrderRequest request);

		/// <summary>
		/// Rolls back (cancels/undoes) an order previously created via <see cref="CreateOrderAsync(CreateOrderRequest)"/> using the <see cref="ZipPaymentFlow.Auth"/> flow.
		/// </summary>
		/// <param name="request">A <see cref="RollbackOrderRequest"/> containing details of the order to rollback.</param>
		/// <returns>A task that can be awaited to know when the operation has completed. If the task does not return an exception, the auth rolled back succesfully. Use the <see cref="GetOrderStatusAsync(OrderStatusRequest)"/> to confirm.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		Task RollbackOrderAsync(RollbackOrderRequest request);

		/// <summary>
		/// Allows retrieval of the client id and secret used to request new auth tokens using the Zip device enrolment system.
		/// </summary>
		/// <param name="request">A <see cref="EnrolRequest"/> instance providing details of the device to enrol.</param>
		/// <remarks>
		/// <para>NOTE: The 'enrollment system' is untested as the test account provided to the library authors did not support generating activation codes. This should work, but if it doesn't, please log an issue.</para>
		/// </remarks>
		/// <returns>A <see cref="EnrolResponse"/> instance containing details of the token returned.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> or any required sub-property is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown if any sub-property of <paramref name="request"/> is determined to be invalid.</exception>
		/// <exception cref="ZipApiException">Thrown if the Zip API returns an error response.</exception>
		/// <exception cref="UnauthorizedAccessException">Thrown if the request to Zip is unauthorised, or if insufficient/incorrect client authentication details have been provided via the <see cref="ZipClientConfiguration"/>.</exception>
		Task<EnrolResponse> EnrolAsync(EnrolRequest request);
	}
}
