# Yort.Zip.InStore

# WORK IN PROGRESS

# Status
![Yort.Zip.InStore.Build](https://github.com/Yortw/Yort.Zip.InStore/workflows/Yort.Zip.InStore.Build/badge.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)  [![Coverage Status](https://coveralls.io/repos/github/Yortw/Yort.Zip.InStore/badge.svg?branch=master)](https://coveralls.io/github/Yortw/Yort.Zip.InStore?branch=master)

# Supported Platforms

* .Net Standard 2.0
* .Net 4.72+

# Documentation

Make sure to read and understand the official [Zip API documentation](https://docs-nz.zip.co/instore-api/api-reference) as this library is a wrapper around that, and reading that 
will give you a good idea of the process flows, how the API works and what is possible. Also see the errata section at the end of this readme.

For getting started, samples and API reference specifically about this library [see the docs](https://yortw.github.io/Yort.Zip.InStore/docs/index.html)

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
    
# Notes/Comments/Errata

At the time this library was initially developed the official [Zip API documentation](https://docs-nz.zip.co/instore-api/api-reference) has a number of missing details, errors and other minor niggles. Known issues with that documentation, and this library are listed here. These issues have been communicated to Zip and they have indicated they will review their documentation and modify as appropriate in the future.

* 'Quantity' and 'Price' for an 'item' as part of an order are listed in the Zip documentation as strings. However sending a quantity that cannot be converted to an integer will fail. This library  exposes quantity as an integer and price as a decimal value, as this seems to actually work and be what the API expects. 

* Specifying a 'StoreId' on requests (or as a default via a *ZipClientConfiguration* instance) using this library will set the 'store-id' HTTP header on requests to the Zip API. This will result in a 401 unauthorised response if your Zip client account isn't configured for this style of access. When you request your client id and secret from Zip you must tell them you want to use the style of auth the allows passing a store on requests, and must have the list of valid stores pre-registered in the zip account. Using a store not registered with Zip (which can't be done yourself through the API, though may be possible via the merchant portal) will also fail. 

* If using a 'StoreId' the value required is the *Zip generated id* for the store, not the name/id of the store you provided to Zip to be registered. You should be able to find the Zip id for a store inside your merchant portal.

* You *cannot* refund an order that was taken using one of the automated/canned response pre-approval codes (i.e AA00) in the test environment. Attempting to refund an order using one of the canned pre-approval codes will result in a *401 Unauthorised* response with no details, which can be confusing. This is not mentioned in the documentation. You *can* refund orders in the test environment created using a pre-approval code generated via a test environment consumer account/login.

* The 'Enrol' API endpoint has been implemented in this library but is not tested. At the time of writing we could not figure out how to generate the 'activation code' from within the test merchant portal, and on querying this we were advised that no integration has needed it anyway. If you want to use this feature and discover a problem, please open an issue and/or file a pull request.

* Some of the success response codes indicated in the official docs are incorrect, for example some endpoints return *202 Accepted* instead of the documented *200 OK*. In these cases this library has been written to accept a response using either the documented or actual status code (to allow for future revisions/changes to the API).

* The Zip API returns some error response bodies in different json formats than others. This library does it's best to support the superset of data returned from all formats seen during development, and attempts to pick the best error message/detail to place in any thrown exception. If you see this code and wonder why it's messy... the multiple formats with no hint of which to expect (content type is always just 'application/json') this is why.

* The Zip AP documentation states:
*Refunds are idempotent, and a merchant refund reference can only be used once. Attempting to use the same reference will result in an error.*
However only the first part of this statement appears to be true - the endpoint does behave in an idempotent fashion, and does *not* send an error back if you repeat a refund request. Instead, sending another request with the same merchant reference will return a copy of the original response. This is actually preferable to returning an error in most cases and actually idempotent (an error response wouldn't be, really), but is a deviation from what the documentation states.
