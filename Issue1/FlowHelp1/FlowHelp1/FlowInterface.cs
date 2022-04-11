using Flow.Net.Sdk;
using Flow.Net.Sdk.Cadence;
using Flow.Net.Sdk.Client;
using Flow.Net.Sdk.Crypto;
using Flow.Net.Sdk.Exceptions;
using Flow.Net.Sdk.Models;

namespace FlowHelp1
{
    public class FlowInterface
    {
        private const uint KEY_INDEX = 0;
        private const uint DEFAULT_GAS_LIMIT = 100;

        private readonly FlowAddress _delphaiAddress;
        private readonly FlowClientAsync _flowClient;
        private readonly ISigner _signer;

        public FlowInterface(FlowClientAsync flowClient, FlowAccount serviceAccount)
        {
            _delphaiAddress = serviceAccount.Address;
            _flowClient = flowClient;

            var serviceAccountKey = serviceAccount.Keys.FirstOrDefault(w => w.Index == KEY_INDEX);
            if (serviceAccountKey == null)
                throw new Exception("No Key Found");

            _signer = serviceAccountKey.Signer;
        }

        public async Task<FlowTransactionResult> ExecuteTransaction(
            string scriptName,
            List<ICadence>? arguments = default,
            Dictionary<string, string>? addressMap = default,
            uint gasLimit = DEFAULT_GAS_LIMIT)
        {
            try
            {
                var txBody = Utilities.ReadCadenceScript(scriptName);

                // Get the latest sealed block to use as a reference block
                var latestBlock = await _flowClient.GetLatestBlockHeaderAsync();

                // Get the latest account info for this address
                var delphaiAccount = await _flowClient.GetAccountAtLatestBlockAsync(
                    _delphaiAddress);

                // Get the latest sequence number for this key
                var delphaiKey = delphaiAccount.Keys.FirstOrDefault(w => w.Index == KEY_INDEX);
                if (delphaiKey == null)
                    throw new Exception("No Key Found");
                var sequenceNumber = delphaiKey.SequenceNumber;

                var tx = new FlowTransaction
                {
                    Script = txBody,
                    GasLimit = gasLimit,
                    ProposalKey = new FlowProposalKey
                    {
                        Address = _delphaiAddress,
                        KeyId = KEY_INDEX,
                        SequenceNumber = sequenceNumber
                    },
                    Payer = _delphaiAddress,
                    ReferenceBlockId = latestBlock.Id,
                    Authorizers = new List<FlowAddress>()
                {
                    _delphaiAddress
                },
                    Arguments = arguments ?? new List<ICadence>(),
                    AddressMap = addressMap ?? new Dictionary<string, string>()
                };

                tx = FlowTransaction.AddEnvelopeSignature(tx, _delphaiAddress, KEY_INDEX, _signer);

                var rawResponse = await _flowClient.SendTransactionAsync(tx);

                await _flowClient.WaitForSealAsync(rawResponse);

                var response = await _flowClient.GetTransactionResultAsync(rawResponse.Id);
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                    throw new FlowException(response.ErrorMessage);

                return response;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }
    }
}
