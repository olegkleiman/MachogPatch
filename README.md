# Send/Read from Azure Queue or Service Bus

## Description
The project demonstrates how to send and read messages from Azure Queue Storage or Azure Service Bus using C#. 
It includes examples for both sending and receiving messages, along with error handling and logging.
It exposes HTTP-bound Azure function - EnqueueMessage - that sends messages to the 
- Storage Accoiunt queue
- Service Bus Queue
- Service Bus Topic.

## Getting Started
- Create local.settings.json file in the root of the project with the following content:
```json
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "xxx",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",

    "QueueName": "parking-provider",

    "SBConnectionString": "xxx",
    "SBQueueName": "parking-provider",
    "SQTopicName": "topic1",

    "ParkingProviderOptions:url": "https://apimtlvppr.tel-aviv.gov.il/qa/payments-reports",
    "ParkingProviderOptions:subscriptionHeader": "Ocp-Apim-Subscription-Key",
    "ParkingProviderOptions:headerValue": "xxx"
  },
  "Host": {
    "CORS": "*"
  }
}
```
- Ensure you have an Azure Subscription with Functions, Service Bus, and Storage Accounts.
- You can run the functions locally from VS Studio 2022 or
```
func start
```

## Usage
Create the Azure Service Bus namespace with Topic named 'topic1' and Subscription named 'subs1' (as hardcoded in ReceiveTopicFunction.cs);
Add the filter to the created subscription looks like 

```
app_id = 'hr'
```


## License
Include licensing information if applicable.