using Flow.Net.Sdk;
using Flow.Net.Sdk.Cadence;
using Flow.Net.Sdk.Client;
using Flow.Net.Sdk.Models;

namespace FlowHelp3
{
    public class BlockExample
    {
        private const string LISTING_EVENT = "A.4eb8a10cb9f87357.NFTStorefront.ListingAvailable";
        private const string WITHDRAW_EVENT = "A.329feb3ab062d289.UFC_NFT.Withdraw";
        private const string DEPOSIT_EVENT = "A.329feb3ab062d289.UFC_NFT.Deposit";
        private const string TOKENS_WITHDRAWN = "A.ead892083b3e2c6c.DapperUtilityCoin.TokensWithdrawn";

        public async Task RunExampleAsync()
        {
            var client = new FlowClientAsync("access.mainnet.nodes.onflow.org:9000");

            await client.PingAsync();

            var latestBlock = await client.GetLatestBlockAsync();
            ulong lastHeight = latestBlock.Height - 1;

            while (true)
            {
                try
                {
                    var block = await client.GetLatestBlockAsync();

                    if (block.Height > lastHeight)
                    {
                        var events = await client.GetEventsForHeightRangeAsync(LISTING_EVENT, lastHeight + 1, block.Height);

                        PrintEvents(events);

                        lastHeight = block.Height;
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static void PrintEvents(IEnumerable<FlowBlockEvent> flowBlockEvents)
        {
            foreach (var blockEvent in flowBlockEvents)
            {
                Console.WriteLine("\n------------------------------------------------------------------------------------------------------------");                
                if(blockEvent.Events.Any())
                {
                    Console.WriteLine($"{blockEvent.Events.Count()} event(s) at block height: {blockEvent.BlockHeight}\n");
                    PrintEvent(blockEvent.Events);
                }
                else
                {
                    Console.WriteLine($"No events at block height: {blockEvent.BlockHeight}");
                }
                Console.WriteLine("------------------------------------------------------------------------------------------------------------\n");
            }                
        }

        private static void PrintEvent(IEnumerable<FlowEvent> flowEvents)
        {
            foreach (var @event in flowEvents)
            {
                Console.WriteLine($"Type: {@event.Type}");
                Console.WriteLine($"Values: {@event.Payload.Encode()}");
                Console.WriteLine($"Transaction ID: {@event.TransactionId.FromByteStringToHex()}");
            }
        }
    }
}
