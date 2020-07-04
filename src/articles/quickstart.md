# Yort.Zip.InStore Quick Start

## Creating an Order (Taking Payment)
### Basic Process
1. Create an instance of [ZipClient](Yort.Zip.InStore.ZipClient.html)
2. Call [ZipClient.CreateOrderAsync](Yort.Zip.InStore.ZipClient.html#Yort_Zip_InStore_ZipClient_CreateOrderAsync_Yort_Zip_InStore_CreateOrderRequest_)
3. If the response from of step 2 has a success status, use [ZipClient.GetStatus](Yort.Zip.InStore.ZipClient.html#Yort_Zip_InStore_ZipClient_GetStatus_Yort_Zip_InStore_OrderStatusRequest_) to poll at one second (or longer) intervals until an accepted, declined or cancelled status is reached. Prior to the order being accepted, declined or cancelled Zip will return a status with a result of *Error* and a message of *Order not found*. Handle any transient errors thrown from the polling request.
4. If you decide to timeout/give up polling, call [ZipClient.Cancel](Yort.Zip.InStore.ZipClient.html#Yort_Zip_InStore_ZipClient_Cancel_Yort_Zip_InStore_CancelOrderRequest_).
5. Once a final status is reached, you can determine whether payment was approved or not by checking the outcome. You need to record the *OrderId* of the result somewhere if you want to be able to process refunds in the future.

**NB This is only a simple/demo implementation. A production quality implementation needs much more robust logic to handle power failure/crash/network outages and other problems.**

### Add the Nuget package
Install the Nuget package;

```powershell
    PM> Install-Package Yort.Zip.InStore
```

[![NuGet Badge](https://buildstats.info/nuget/Yort.Zip.InStore)]

### Full Code Sample
A full (demo quality) code sample for creating payment, with minimum comments/noise.
See the following sections for a breakdown of what's going on and why.

```c#
    IZipClient client = new ZipClient
    (
        new ZipClientConfiguration("YourClientId", "YourClientSecret", ZipEnvironment.NewZealand.Test)
    );    

    var request = new CreateOrderRequest()
    {
        TerminalId = "2531",
        Order = new ZipOrder()
        {
            Amount = 10.5M,
            CustomerApprovalCode = "AA05", 
            MerchantReference = Guid.NewGuid().ToString(), 
            Operator = "Kermit The Frog",
            PaymentFlow = ZipPaymentFlow.Payment,
        }
    };    

    CreateOrderResponse result = await client.CreateOrderAsync(request);

    var statusRequest = new OrderStatusRequest() { OrderId = result.OrderId };

    var done = false;
    while (!done)
    {
        await Task.Delay(1000);
        status = await client.GetOrderStatusAsync(statusRequest);
        if (status.Result == ZipOrderStatus.Cancelled) throw new OperationCanceledException();
        done = ZipOrderStatus.IsTerminalStatus(statusResponse.Status);
    }

    if (statusResponse.Status != ZipOrderStatus.Complete) 
        throw new InvalidOperationException("Payment failed: " + statusResponse.Status);

    return status.OrderId;
```

### Configure and Create A ZipClient Instance
The ZipClient is the main class in this library to use for accessing the Zip API functionality. To create an instance first you must create a [ZipClientConfiguration](Yort.Zip.InStore.ZipClientConfiguration.html) object that tells the client how to behave. This sample configures a client for accessing the sandbox/text version of the API.

You'll need a client id and secret issued by Zip for the sandbox environment for this to work.

Normally you would create the Zip instance on start-up or first use, and then re-use it across requests instead of creating a new one each time. This allows HTTP connection pooling and improves performance. The ZipClient instance is thread-safe in so much as you can run multiple requests through the same instance for different payments from different threads.

```c#
    //Zip will issue a client id and secret for each environment,
    //you need to contact Zip to get these.
    var config = new ZipClientConfiguration
    (
        "YourClientId", 
        "YourClientSecret", 
        ZipEnvironment.NewZealand.Test
    );

    //We use IZipClient as the type for the variable holding 
    //the ZipClient instance here as it makes it easier if we 
    //want a test implementation later.
    IZipClient client = new ZipClient(config);    
```

### Call Create Order
Now we can create an order. Create a CreateOrderRequest with details of the payment to take, and pass it to the [ZipClient.CreateOrderAsync](Yort.Zip.InStore.ZipClient.html#Yort_Zip_InStore_ZipClient_CreateOrderAsync_Yort_Zip_InStore_CreateOrderRequest_) method.

```c#
    //We'll set the minimum required properties for this example
    var request = new CreateOrderRequest()
    {
        TerminalId = "2531",
        Order = new ZipOrder()
        {
            Amount = 10.5M,
            CustomerApprovalCode = "AA05", 
            MerchantReference = Guid.NewGuid().ToString(), 
            Operator = "Kermit The Frog",
            PaymentFlow = ZipPaymentFlow.Payment,
        }
    }; 

    //Note, this method could throw exceptions. Production quality code
    //would handle them appropriately. 
    CreateOrderResponse result = await client.CreateOrderAsync(request);
```

### Poll for Status
Now we need to wait for the customer to approve or decline the payment request via the Zip app or website. If they approve the payment then it will be sent by Zip to the payment processor, and if the payment processor accepts then we'll get a completed status. If either the customer or the payment processor declines the response we'll get a cancel/declined or error response.

```c#
    var done = false;
    // Create a status request. You can create a new one on each loop or 
    // reuse the same one, which is more efficient so that's what we do here.
    // Unfortunately the Zip status endpoint doesn't use the merchant reference, but 
    // instead the order id returned by the initial create request. If that initial 
    // request is interrupted by a crash, power failure, timeout/network error etc. 
    // you must retry it with duplicate checking enabled until such time as you 
    // receive an order id you can status check against.
    var statusRequest = new OrderStatusRequest() { OrderId = result.OrderId };

    OrderStatusResponse status = null;

    //Production quality code would also have a timeout, in case the user
    //is unable to approve or cancel the order in a timely fashion at 
    //the till. There should also be an option for the operator to manually 
    //cancel, in the case where the payment amount has been incorrectly entered 
    //or the consumer is unable to cancel themselves.
    var done = false;
    while (!done)
    {
        await Task.Delay(1000); // Wait one second between poll requests
        //Request the current status
        status = await client.GetOrderStatusAsync(statusRequest);
        //Detect if we got a final status
        if (status.Result == ZipOrderStatus.Cancelled) throw new OperationCanceledException();
        done = ZipOrderStatus.IsTerminalStatus(statusResponse.Status);
    }

    //If the payment reached a final status other than complete, throw an exception to notify the caller. 
    if (statusResponse.Status != ZipOrderStatus.Complete) 
        throw new InvalidOperationException("Payment failed: " + statusResponse.Status);

    // At this point if the customer approved the payment then status.Result 
    // should be "Completed" and the payment plan should be in place. 
    // You need to keep the Zip order id for future use
    // with refunds.
    return status.OrderId;  
```

## Cancelling an Order
To cancel a Zip order that is still pending approval (not accepted, declined or cancelled) you will need the *OrderId* returned in the [CreateOrderResponse](Yort.Zip.InStore.CreateOrderResponse.html). Cancellation is successful if the CancelOrderAsync method does not throw an exception. The CancelOrderRequest instance returned will contain the order id of the cancelled order as confirmation.

```c#
    try
    {
        var cancelRequest = new CancelOrderRequest() { OrderId = createOrderResult.OrderId, Operator = "Test", TerminalId = "2531" };
	    var cancelResponse = await client.CancelOrderAsync(cancelRequest);
        //Cancellation succeeded
    }
    catch (Exception) // Production code should catch specific exception types
    {
        //Cancellation failed
    }
```

## Refunding an Order
Once an order has been approved you can make one or more refunds against it, up to the total value of the original order. For each refund create a RefundOrderRequest, specify the order id that was returned when the order was created, the refund amount and a unique reference for the refund request. If you get interrupted (power failure, crash, network outage) or suffer a transient error sending a refund, resend the same request again until you get a valid response. Using the same refund details on the retry requests should ensure idempotency and prevent multiple refunds being created if the earlier attempts did actually succeed.

```c#
    var createRefundRequest = new RefundOrderRequest() 
    {
        MerchantRefundReference = System.Guid.NewGuid().ToString(), 
        OrderId = createOrderResult.OrderId,
        Amount = 1, 
        Operator = "Test", 
        TerminalId = "2531" 
    };

    try
    {
        var refundResponse = await client.RefundOrderAsync(refundRequest);
        //Refund succeeded, refundResponse.Id will contain the id of the new refund.
    }
    catch (Exception) // Production code should catch specific exception types
    {
        //Refund failed
    }
```
