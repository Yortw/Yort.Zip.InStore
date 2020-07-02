# Yort.Zip.InStore

# WORK IN PROGRESS

# Status
![Yort.Zip.InStore.Build](https://github.com/Yortw/Yort.Zip.InStore/workflows/Yort.Zip.InStore.Build/badge.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![Coverage Status](https://coveralls.io/repos/github/Yortw/Yort.Zip.InStore/badge.svg?branch=master)](https://coveralls.io/github/Yortw/Yort.Zip.InStore?branch=master)

# Supported Platforms

* .Net Standard 2.0
* .Net 4.72+

# Documentation

For getting started, samples and API reference [see the docs](https://yortw.github.io/Yort.Zip.InStore/docs/index.html)

## Sample code so you know what you're getting into

Because everyone really wants a sample in the readme...
Here's some example code for creating a Zip order (payment request) using this library.

There's really only three steps:
* Obtain or create a ZipClient instance
* Create the order request
* Send the order request to Zip using the client created in step 1... handle the result or any exceptions thrown.

```c#
    // In production you'd probably inject a ZipClient instance or use a pre-configured, shared instance, 
    // depending on your use case and architecture. For the sake of a sample, here's what creating one looks like.
    var client = new ZipClient
    (
        new ZipClientConfiguration
        (
            "YourClientId", //Client id and secret are obtained from Zip by applying for access to their API.
            "YourClientSecret",
            ZipEnvironment.NewZealand.Test
        )
    );    

    //Define the "order" (payment request) we want Zip to create. The 'Items' collection is optional
    //but provides a better end-consumer experience when provided.
    var request = new CreateOrderRequest()
    {
        StoreId = "Albany", //Optional, depending on the Zip auth mechanism you registered for
        TerminalId = "2531",
        Order = new ZipOrder()
        {
            Amount = 10.5M,
            CustomerApprovalCode = "AA05", // This is provided by the customer from their app, as text or via scanning QR code
            MerchantReference = Guid.NewGuid().ToString(), //Normally you'd do a better job of storing this and using it for crash recovery, but this is only sample code
            Operator = "Kermit The Frog",
            PaymentFlow = ZipPaymentFlow.Payment,
            Items = new List<ZipOrderItem>()
            {
                new ZipOrderItem()
                {
                    Name = "Test Item",
                    Description = "0110A Blue 12",
                    Price = 10.50M,
                    Quantity = 1, //Zip only allows integer quantities :()
                    Sku = "123"
                }
            }
        }
    };    

    // Now send the request
    // A successful request will contain the Zip order id that can be used to poll for the payment status,
    // otherwise an exception is thrown.
    // Again, production code would do a better job by writing the zip order id to storage so it can be used
    // for crash/power failure/comms failure recovery. This is just sample code.
    try
    {
	    CreateOrderResponse result = await client.CreateOrderAsync(request);
        //TODO: wait on a final status or timeout and cancel etc.
    }
    catch (UnauthorizedAccessException)
    {
        //TODO: Report a problem with the credentials
    }
    catch (ZipApiException zipEx)
    {
        //TODO: Report a failure, i.e request over credit limit, missing required value etc.
    }
```
    
