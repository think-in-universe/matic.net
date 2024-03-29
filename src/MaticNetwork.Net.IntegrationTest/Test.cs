﻿using System;
using Xunit;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using MaticNetwork.Net;

namespace MaticNetwork.Net.IntegrationTest
{
    public class MaticAPITest
    {

        public IMatic GetMatic()
        {
            var matic = new Matic(Config.MATIC_PROVIDER, Config.PARENT_PROVIDER, Config.SYNCER_URL, Config.WATCHER_URL, Config.ROOTCHAIN_ADDRESS, Config.MATICWETH_ADDRESS, Config.WITHDRAWMANAGER_ADDRESS, Config.DEPOSITMANAGER_ADDRESS);
            matic.SetPrivateKey(Config.PRIVATE_KEY);
            return matic;
        }

        // read info

        async Task BalanceOfEth()
        {
            var web3 = new Web3(Config.MATIC_PROVIDER);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(Config.FROM_ADDRESS);
            Console.WriteLine($"Balance in Wei: {balance.Value}");

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");
        }

        [Fact]
        public async Task GetMappedTokenAddress() {
            var matic = GetMatic();
            var mappedAddress = await matic.GetMappedTokenAddress(Config.ROPSTEN_TEST_TOKEN);
            Console.WriteLine($"The mapped address is {mappedAddress}");
        }

        [Fact]
        public async Task BalanceOfERC20() {
            var matic = GetMatic();
            var balance = await matic.BalanceOfERC20(Config.FROM_ADDRESS, Config.MATIC_TEST_TOKEN);
            Console.WriteLine($"BalanceOfERC20 is {balance}");
        }

        [Fact]
        public async Task BalanceOfERC721() {
            var matic = GetMatic();
            var balance = await matic.BalanceOfERC721(Config.FROM_ADDRESS, Config.MATIC_ERC721_TOKEN);
            Console.WriteLine($"BalanceOfERC721 of matic chain is {balance}");

            balance = await matic.BalanceOfERC721(Config.FROM_ADDRESS, Config.ROPSTEN_ERC721_TOKEN, parent: true);
            Console.WriteLine($"BalanceOfERC721chain is {balance}");
        }

        public async Task TokenOfOwnerByIndexERC721()
        {
            var matic = GetMatic();
            var tokenId = await matic.TokenOfOwnerByIndexERC721(Config.FROM_ADDRESS, Config.MATIC_ERC721_TOKEN, 0, false);
            Console.WriteLine($"TokenOfOwnerByIndexERC721 of matic is {tokenId}");

            tokenId = await matic.TokenOfOwnerByIndexERC721(Config.FROM_ADDRESS, Config.ROPSTEN_ERC721_TOKEN, 0, true);
            Console.WriteLine($"TokenOfOwnerByIndexERC721 of parent chain is {tokenId}");
        }

        // deposit

        public async Task DepositERC20Token()
        {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.ROPSTEN_TEST_TOKEN;
            var amount = 1000000000000000000;
            await matic.ApproveERC20TokensForDeposit(from, token, amount);
            Console.WriteLine($"ApproveERC20TokensForDeposit finished");
            await matic.DepositERC20Tokens(from, from, token, amount);
            Console.WriteLine($"DepositERC20Tokens finished");
        }

        public async Task DepositERC721Token()
        {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.ROPSTEN_ERC721_TOKEN;
            var tokenId = 696;
            await matic.ApproveERC721TokensForDeposit(from, token, tokenId);
            Console.WriteLine($"ApproveERC721TokensForDeposit finished");
            await matic.DepositERC721Tokens(from, from, token, tokenId);
            Console.WriteLine($"DepositERC721Token finished");
        }

        public async Task SafeDepositERC721Tokens() {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.ROPSTEN_ERC721_TOKEN;
            var tokenId = 1004;
            await matic.SafeDepositERC721Tokens(from, token, tokenId);
            Console.WriteLine($"SafeDepositERC721Tokens finished");
        }

        public async Task DepositEthers()
        {
            var matic = GetMatic();
            var amount = 1000000000000000; // 0.001 Ether
            await matic.DepositEthers(Config.FROM_ADDRESS, amount);
            Console.WriteLine($"DepositEthers finished");
        }

        // transfer

        public async Task TransferTokens() {
            var matic = GetMatic();
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

        public async Task TransferERC721Tokens() {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var to = Config.TO_ADDRESS;
            var token = Config.MATIC_ERC721_TOKEN;
            var tokenId = 1001;
            await matic.TransferERC721Tokens(from, token, to, tokenId);
            Console.WriteLine($"TransferERC721Tokens finished");
        }

        public async Task TransferEthers() {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var to = Config.TO_ADDRESS;
            var amount = 1000000000000000;

            await matic.TransferEthers(from, to, amount);
            Console.WriteLine($"TransferEthers finished");
        }

        // withdraw

        public async Task StartWithdraw() {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var to = Config.TO_ADDRESS;
            var token = Config.MATIC_TEST_TOKEN;
            var amount = 1000000000000000000;

            var balance = await matic.BalanceOfERC20(from, token);
            Console.WriteLine($"Test Token Balance is {balance}");

            var hash = await matic.StartWithdraw(from, token, amount);
            Console.WriteLine($"StartWithdraw finished: {hash}");

            balance = await matic.BalanceOfERC20(from, token);
            Console.WriteLine($"Test Token Balance is {balance}");
        }

        public async Task StartERC721Withdraw() {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.MATIC_ERC721_TOKEN;
            var tokenId = 1002;
            var hash = await matic.StartERC721Withdraw(from, token, tokenId);
            Console.WriteLine($"StartERC721Withdraw finished: {hash}");
        }

        public async Task StartEthersWithdraw()
        {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.MATICWETH_ADDRESS;
            var amount = 1000000000000000; // 0.001 ETH

            var hash = await matic.StartWithdraw(from, token, amount);
            Console.WriteLine($"StartEthersWithdraw finished: {hash}");
        }

        public async Task ConfirmWithdraw()
        {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var txId = "0xba2808123d31caf13a7eee0a5bed1e5861ca7df1815c99ad617eba65df194e5f";
            // var txId = "0xf3869b30ee3a2ea4f7e04fde757a2072d417db5ed131f2df7a4f727d96ff107e";
            // var txId = "0x3b1cc6fd194688b8787686f7e0b9538adf69c1617197d98e97b0732c230e303e";
            // "0xc40f9d7c4e6175e32880e3ee4815fc3bb7052b3397c57f52cbf3aef7fa8b0e83"; // initiate-withdraw-ERC20

            await matic.Withdraw(from, txId);
            Console.WriteLine($"ConfirmWithdraw finished");
        }

        public async Task ProcessExitERC20()
        {
            var matic = GetMatic();
            var from = Config.FROM_ADDRESS;
            var token = Config.ROPSTEN_TEST_TOKEN;

            await matic.ProcessExits(from, token);
            Console.WriteLine($"ProcessExitERC20 finished");
        }

    }
}
