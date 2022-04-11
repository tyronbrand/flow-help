using Flow.Net.Sdk.Client;
using FlowHelp1;

// create client
var _flowClient = new FlowClientAsync("127.0.0.1:3569"); // emulator

//ping to check we are connected
await _flowClient.PingAsync();

// getting address details from flow.json
var emulatorAccount = await _flowClient.ReadAccountFromConfigAsync("emulator-account");

// run it
var flowInterface = new FlowInterface(_flowClient, emulatorAccount);
var result = await flowInterface.ExecuteTransaction("log-signer-address");