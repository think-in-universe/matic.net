using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

namespace MaticNetwork.Net
{
    class TestSuite
    {
        static void Main(string[] args)
        {
            // read

            // BalanceOfEth().Wait();
            // BalanceOfERC20().Wait();
            // GetMappedTokenAddress().Wait();
            // TokenOfOwnerByIndexERC721().Wait();

            // deposit

            DepositEthers().Wait();
            // DepositERC20Token().Wait();
            // DepositERC721Token().Wait();
            // SafeDepositERC721Tokens().Wait();

            // transfer

            // TransferTokens().Wait();
            // TransferERC721Tokens().Wait();
            // TransferEthers().Wait();

            // withdraw


        }

        static IMatic getMatic() {
            var matic = new Matic(Config.MATIC_PROVIDER, Config.PARENT_PROVIDER, Config.SYNCER_URL, Config.WATCHER_URL, Config.ROOTCHAIN_ADDRESS, Config.MATICWETH_ADDRESS, Config.WITHDRAWMANAGER_ADDRESS, Config.DEPOSITMANAGER_ADDRESS);
            matic.SetPrivateKey(Config.PRIVATE_KEY);
            return matic;
        }

        static async Task BalanceOfEth()
        {
            var web3 = new Web3(Config.MATIC_PROVIDER);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(Config.FROM_ADDRESS);
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");
        }

        static async Task GetMappedTokenAddress() {
            var matic = getMatic();
            var mappedAddress = await matic.GetMappedTokenAddress(Config.ROPSTEN_TEST_TOKEN);
            Console.WriteLine($"The mapped address is {mappedAddress}");
        }

        static async Task BalanceOfERC20() {
            var matic = getMatic();
            var balance = await matic.BalanceOfERC20(Config.FROM_ADDRESS, Config.MATIC_TEST_TOKEN);
            Console.WriteLine($"BalanceOfERC20 is {balance}");
        }

        static async Task TokenOfOwnerByIndexERC721()
        {
            var matic = getMatic();
            var tokenId = await matic.TokenOfOwnerByIndexERC721(Config.FROM_ADDRESS, Config.ROPSTEN_ERC721_TOKEN, 0);
            Console.WriteLine($"TokenOfOwnerByIndexERC721 is {tokenId}");
        }

        static async Task DepositERC20Token()
        {
            var matic = getMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.ROPSTEN_TEST_TOKEN;
            var amount = 1000000000000000000;
            await matic.ApproveERC20TokensForDeposit(from, token, amount);
            Console.WriteLine($"ApproveERC20TokensForDeposit finished");
            await matic.DepositERC20Tokens(from, from, token, amount);
            Console.WriteLine($"DepositERC20Tokens finished");
        }

        static async Task DepositERC721Token()
        {
            var matic = getMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.ROPSTEN_ERC721_TOKEN;
            var tokenId = "1";
            await matic.ApproveERC721TokensForDeposit(from, token, tokenId);
            Console.WriteLine($"ApproveERC721TokensForDeposit finished");
            await matic.DepositERC721Tokens(from, from, token, tokenId);
            Console.WriteLine($"DepositERC721Token finished");
        }

        static async Task SafeDepositERC721Tokens() {
            var matic = getMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.ROPSTEN_ERC721_TOKEN;
            var tokenId = "72";
            await matic.SafeDepositERC721Tokens(from, token, tokenId);
            Console.WriteLine($"SafeDepositERC721Tokens finished");
        }

        static async Task DepositEthers()
        {
            var matic = getMatic();
            var amount = 1000000000000000; // 0.001 Ether
            await matic.DepositEthers(Config.FROM_ADDRESS, amount);
            Console.WriteLine($"DepositEthers finished");
        }

        static async Task TransferTokens() {
            var matic = getMatic();
            var from = Config.FROM_ADDRESS;
            var to = Config.TO_ADDRESS;
            var token = Config.MATIC_TEST_TOKEN;
            var amount = 1000000000000000000;

            var balance = await matic.BalanceOfERC20(from, token);
            Console.WriteLine($"Test Token Balance is {balance}");

            await matic.TransferTokens(from, token, to, amount);
            Console.WriteLine($"TransferTokens finished");

            balance = await matic.BalanceOfERC20(from, token);
            Console.WriteLine($"Test Token Balance is {balance}");
        }

        static async Task TransferERC721Tokens() {
            var matic = getMatic();
            var from = Config.FROM_ADDRESS;
            var to = Config.TO_ADDRESS;
            var token = Config.MATIC_ERC721_TOKEN;
            var tokenId = "1";
            await matic.TransferERC721Tokens(from, token, to, tokenId);
            Console.WriteLine($"TransferERC721Tokens finished");
        }

        static async Task TransferEthers()
        {
            var matic = getMatic();
            var from = Config.FROM_ADDRESS;
            var to = Config.TO_ADDRESS;
            var amount = 1000000000000000;

            await matic.TransferEthers(from, to, amount);
            Console.WriteLine($"TransferEthers finished");
        }

    }
}
