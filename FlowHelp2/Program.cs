using Flow.Net.Sdk;
using Flow.Net.Sdk.Cadence;
using Flow.Net.Sdk.Client;

// basic example getting NFL All Day Deposit events

// create client
var flowClient = new FlowClientAsync("access.mainnet.nodes.onflow.org:9000");

// example will stop when we hit the latestBlock height
var latestBlock = await flowClient.GetLatestBlockAsync();

ulong startBlockHeight = 27341470;
bool reachedCurrentBlockHeight = false;

while(!reachedCurrentBlockHeight)
{
    ulong endBlockHeight = startBlockHeight+249; // max 250 events

    if(endBlockHeight >= latestBlock.Height)
    {
        endBlockHeight = latestBlock.Height;
        reachedCurrentBlockHeight = true;
    }

    var eventsForHeightRange = await flowClient.GetEventsForHeightRangeAsync("A.e4cf4bdc1751c65d.AllDay.Deposit", startBlockHeight, endBlockHeight);
    
    foreach (var blockEvent in eventsForHeightRange)
    {
        foreach (var @event in blockEvent.Events)
        {
            Console.WriteLine($"Type: {@event.Type}");
            Console.WriteLine($"Values: {@event.Payload.Encode()}");
            Console.WriteLine($"Transaction ID: {@event.TransactionId.FromByteStringToHex()} \n");
        }
    }

    startBlockHeight += 250;
}