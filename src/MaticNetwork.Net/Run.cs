using System;
using System.Threading.Tasks;
using Nethereum.Web3;

namespace MaticNetwork.Net
{
    class MainClass
    {
        static void Main(string[] args)
        {
            // GetAccountBalance().Wait();
            // GetMappedTokenAddress().Wait();
            // BalanceOfERC20().Wait();
            TokenOfOwnerByIndexERC721().Wait();
            //  Console.ReadLine();
        }

        static IMatic getMatic() {
            return new Matic(Config.MATIC_PROVIDER, Config.PARENT_PROVIDER, Config.SYNCER_URL, Config.WATCHER_URL, Config.ROOTCHAIN_ADDRESS, Config.MATICWETH_ADDRESS, Config.WITHDRAWMANAGER_ADDRESS, Config.DEPOSITMANAGER_ADDRESS);
        }

        static async Task GetAccountBalance()
        {
            var web3 = new Web3("https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
            var balance = await web3.Eth.GetBalance.SendRequestAsync("0xde0b295669a9fd93d5f28d9ec85e40f4cb697bae");
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

    }
}
