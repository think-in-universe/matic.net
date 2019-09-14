# Matic.Net

**Matic.Net** is a .NET Client for Matic Network (https://matic.network/), to supports dApps and innovation via .NET ecosystem including C#, C++, Visual Basic, etc.

Matic.Net makes it easy for developers, who may not be deeply familiar with smart contract development, to interact with the various components of Matic Network.

This library will help developers to move assets from Ethereum chain to Matic chain, and withdraw from Matic to Ethereum using fraud proofs.

Matic.Net follows the design principles of [matic.js](https://github.com/maticnetwork/matic.js), and implements all of `matic.js` APIs.

Matic.Net also natively support Game Development with platforms such as Unity3D.

## APIs

The APIs in C# interface as an example.

```cs
    // API references: https://docs.matic.network/getting-started/

    void SetPrivateKey(string privateKey);

    // info

    Task<string> GetMappedTokenAddress(string tokenAddress);
    Task<string> TokenOfOwnerByIndexERC721(string address, string token, int index, bool parent = false);
    Task<BigInteger> BalanceOfERC20(string address, string token, bool parent = false);
    Task<BigInteger> BalanceOfERC721(string address, string token, bool parent = false);

    // deposit

    Task ApproveERC20TokensForDeposit(string from, string token, BigInteger amount);
    Task DepositERC20Tokens(string from, string user, string token, BigInteger amount);
    Task SafeDepositERC721Tokens(string from, string token, string tokenId);
    Task ApproveERC721TokensForDeposit(string from, string token, string tokenId);
    Task DepositERC721Tokens(string from, string user, string token, string tokenId);
    Task DepositEthers(string from, BigInteger value);

    // transfer

    Task<string> TransferTokens(string from, string token, string user, BigInteger amount, bool parent = false);
    Task<string> TransferERC721Tokens(string from, string token, string user, string tokenId, bool parent = false);
    Task<string> TransferEthers(string from, string to, BigInteger amount, bool parent = false, bool isCutomEth = false);

    // withdraw

    Task<string> StartWithdraw(string from, string token, BigInteger amount);
    Task<string> StartERC721Withdraw(string from, string token, string tokenId);
    Task<string> Withdraw(string from, string txId);
    Task<string> ProcessExits(string from, string rootTokenAddress);

    // transaction

    Task<object> getTx(string txId);
    Task<object> getReceipt(string txId);
    Task<object> getHeaderObject(string blockNumber);

```

## How it works?

The flow for asset transfers on the Matic Network is as follows:

- User deposits crypto assets in Matic contract on mainchain
- Once deposited tokens get confirmed on the main chain, the corresponding tokens will get reflected on the Matic chain.
- The user can now transfer tokens to anyone they want instantly with negligible fees. Matic chain has faster blocks (approximately ~ 1 second). That way, the transfer will be done almost instantly.
- Once a user is ready, they can withdraw remaining tokens from the mainchain by establishing proof of remaining tokens on Root contract (contract deployed on Ethereum chain)


## Getting started

Developers are recommended start with [matic.js](https://docs.matic.network/getting-started/) to learn about the major workflow and examples of working with Matic Network.

### Install

1. Install .Net SDK

Matic.Net works with .Net Core or .Net Framework. You need to have the .Net SDK installed. For newbies we recommend .Net core.

[Download and Install .Net SDK](https://www.microsoft.com/net/download)

2. Craete an App

Create a new project with .Net CLI as below, OR create a project in Visual Studio.

```bash
dotnet new console -o MaticExample
cd MaticExample
```

3. Add Matic.Net package

Add Matic.Net package reference by downloading the packages from NuGet with dotnet command (not ready in NuGet yet, will be added very soon)

```bash
dotnet add package MaticNetwork.Net
dotnet restore
```


### Configuraiton

Developers may refer to [Config.cs](./src/MaticNetwork.Net/Config.cs) as an example of configuring testnet, contracts and accounts information.


## Documentation

To be added


### Game development with Unity3D

Matic.Net supports Game Development with platform which works well with .NET, such as Unity3D.

Look at [Matic Unity Starter Kit](https://github.com/think-in-universe/matic-unity-starter-kit) to learn more about how to develop a modern blockchain game with Unity3D and Matic.Net.


## Dependencies

Matic.Net is built based on [Nethereum](https://github.com/Nethereum/Nethereum) (an Ethereum .Net cross platform integration library) to interact with Ethereum blockchain and Matic sidechain.


## License

MIT License
