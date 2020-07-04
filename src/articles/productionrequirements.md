# Production Implementation Requirements

In order to provide a production quality integration with Zip the application developer must undertake a number of steps.

## Power Failure/Crash Recovery
A reliable integration must handle situation where the POS process dies while a Zip order (or refund) is pending, due to a power/hardware failure, OS restart, bug etc. In this case the usual pattern is:
    * On start-up, or another suitable event, check a persistent store for a list of outstanding requests. For each pending request, recover at least the *MerchantReference*, and preferably the order id (if it was received and persisted from the initial CreateOrder request). 
    * If you are only able to retrieve the merchant reference, retry the order create request ensuring 'duplicate checking' is enabled. This will allow you to retrieve the Zip 
    order id required in the following steps without risk of double charging the customer.
    * If a pending order is found, begin polling for the status of that order. If the order is still pending after the first check either continue polling or cancel immediately. When a final state is reached the POS should relate this answer back to the pending POS transaction, provide some way for the user to apply it to a new transaction or perform an automatic refund depending on how the POS is designed.
    * If a pending refund is found, retry the refund request using the same merchant reference if you want to ensure the refund occurred. Zip refunds requests are idempotent, and sending the same request again will only result in at most one refund being applied. 
    * Before sending a CreateOrderRequest or RefundOrderRequest, generate a unique MerchantReference to be used on that request and save it to persistent storage so it can be used for crash recovery (see above). For a CreateOrderRequest also store the order id returned as soon as possible if the call is successful.
    * When a POS transaction is completed (or next persisted to permanent storage) with an order or refund that has reached a final status, remove that requests reference from the persistent store of pending requests so it will no longer be rechecked on a restart (see previous steps).

## Error Handling
All methods in the ZipClient class might throw exceptions. Some exceptions such as System.ArgumentException and System.ArgumentNullException occur before the request is sent, if the request data doesn't meet the minimum known requirements for Zip. These should be handled probably presented to the user to tell them what is missing.

In addition, any error normally thrown by the .Net HttpClient class including System.TaskCanceledException and HttpRequestException could be re-thrown from these methods. TaskCanceled and timeout exceptions may require special handling. For example if these exceptions are thrown during a CreateOrderRequest call and the POS is not going to try again then another call should be made to CancelOrder to ensure the order is not subsequently accepted by the user.

HttpResponseExceptions may indicate a transient error, such as the network being briefly down, or a service unavailable/too many requests response from the server. In this case the POS should wait a short period and then retry the operation.

Any error returned by the Zip API (indicating the HTTP call was received by the server but resulted in an error) is converted to a ZipApiException, with an Errors property that contains details of the error response.

## Polling Timeout
It's possible that due to a long network outage or similar problem that a create order request may never reach an approved or declined state and cannot be immediately cancelled. In this case the POS should have a timeout where it gives up polling. This timeout should not be too short, or should prompt the operator to ask if it should give up, as new customers can sometimes take several minutes to complete the sign up process. It is up to the POS to decide how to behave, but the usual logic is:
* Treat the request as declined
* Store the request somewhere to be rechecked at a later time when the network is available again.
* If the customer still wants to make payment they will have to choose another payment method (since the network is not available and Zip cannot be used)
* On recheck (once the network is available again):
    * If the payment is pending, cancel it.
    * If the payment is approved, refund it.
    * If the payment is cancelled or declined, there is nothing to do except remove it from the list of rechecks

## Manual Cancellation
It is possible the POS operator may need to cancel an in-progress payment, if the end consumer is unable to do so for some reason. It's also possible the customer's device may run out of battery, have no network connection to receive the payment request, or get dropped and damaged etc. In any of these cases where the payment cannot be processed for a known reason the POS user should have the option to manually cancel the request without waiting for a long timeout.  This needs to use the CancelOrder method of the ZipClient object to notify Zip of the cancellation, and only treat the payment as cancelled if the cancellation request is successful.

## Cancelling Orders
When making a CancelOrder request an exception might be thrown, and this could be caused by the order having been approved or declined between when you last performed a status check and when the cancellation request was received. If an ZipApiException is thrown as the result of a cancellation request your code should re-check the current status of the order and take action depending on the latest status.