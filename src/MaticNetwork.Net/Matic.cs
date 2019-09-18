using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using Nethereum.Util;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json.Linq;

namespace MaticNetwork.Net
{
    public class Matic: IMatic
    {

        private string _maticProvider;
        private string _parentProvider;
        private IWeb3 _childWeb3;
        private IWeb3 _parentWeb3;
        private string _syncerUrl;
        private string _watcherUrl;
        private string _rootChainAddress;
        private string _maticWethAddress;
        private string _withdrawManagerAddress;
        private string _depositManagerAddress;

        // contracts
        private Contract _rootChainContract;
        private Contract _withdrawManagerContract;
        private Contract _depositManagerContract;

        // internal cache
        private Dictionary<string, Contract> _tokenCache = new Dictionary<string, Contract>();
        private Dictionary<string, string> _tokenMappedCache = new Dictionary<string, string>();

        // artifacts
        private string RootChainArtifacts;
        private string ChildERC20Artifacts;
        private string ChildERC721Artifacts;
        private string StandardTokenArtifacts;
        private string WithdrawManagerArtifacts;
        private string DepositManagerArtifacts;

        private static readonly HttpClient webClient = new HttpClient();

        public Matic (string maticProvider, string parentProvider, string syncerUrl = null, string watcherUrl = null, string rootChainAddress = null, string maticWethAddress = null, string withdrawManagerAddress = null, string depositManagerAddress = null) {

            this._maticProvider = maticProvider;
            this._parentProvider = parentProvider;
            this._childWeb3 = new Web3(maticProvider);
            this._parentWeb3 = new Web3(parentProvider);

            this._syncerUrl = syncerUrl;
            this._watcherUrl = watcherUrl;
            this._rootChainAddress = rootChainAddress;
            this._maticWethAddress = maticWethAddress;
            this._withdrawManagerAddress = withdrawManagerAddress;
            this._depositManagerAddress = depositManagerAddress;

            // read artifacts
            this.RootChainArtifacts = new ArtifactsReader("RootChain").GetABI();
            this.WithdrawManagerArtifacts = new ArtifactsReader("WithdrawManager").GetABI();
            this.DepositManagerArtifacts = new ArtifactsReader("DepositManager").GetABI();
            this.StandardTokenArtifacts = new ArtifactsReader("StandardToken").GetABI();
            this.ChildERC20Artifacts = new ArtifactsReader("ChildERC20").GetABI();
            this.ChildERC721Artifacts = new ArtifactsReader("ChildERC721").GetABI();

            this._updateContracts();
        }

        public IWeb3 ChildWeb3()
        {
            return this._childWeb3;
        }

        public IWeb3 ParentWeb3()
        {
            return this._parentWeb3;
        }

        public void SetPrivateKey(string privateKey) {
            this._childWeb3 = new Web3(new Account(privateKey), this._maticProvider);
            this._parentWeb3 = new Web3(new Account(privateKey), this._parentProvider);
            this._updateContracts();
        }

        private void _updateContracts() {
            // create rootchain contract
            this._rootChainContract = this.ParentWeb3().Eth.GetContract(
                this.RootChainArtifacts,
                this._rootChainAddress
            );

            // create withdraw manager contract
            this._withdrawManagerContract = this.ParentWeb3().Eth.GetContract(
                this.WithdrawManagerArtifacts,
                this._withdrawManagerAddress
            );

            // create deposit manager contract
            this._depositManagerContract = this.ParentWeb3().Eth.GetContract(
                this.DepositManagerArtifacts,
                this._depositManagerAddress
            );
        }

        public async Task<string> GetMappedTokenAddress(string address)
        {
            var _a = address.ToLower();
            if (!this._tokenMappedCache.ContainsKey(_a))
            {
                // var mappedAddress = await this._depositManagerContract.methods.tokens(_a).call();
                var tokens = this._depositManagerContract.GetFunction("tokens");
                var mappedAddress = await tokens.CallAsync<string>(_a);
                this._tokenMappedCache.Add(_a, mappedAddress);
            }
            return this._tokenMappedCache[_a];
        }

        public async Task<BigInteger> BalanceOfERC721(string address, string token, bool parent = false) {
            var web3Object = parent ? this.ParentWeb3() : this.ChildWeb3();
            var contract = this._GetERC721TokenContract(token, web3Object);
            var balance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(address);
            return balance;
        }

        public async Task<BigInteger> BalanceOfERC20(string address, string token, bool parent = false)
        {
            var web3Object = parent ? this.ParentWeb3() : this.ChildWeb3();
            var contract = this._GetERC20TokenContract(token, web3Object);
            var balance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(address);
            return balance;
        }

        public async Task<BigInteger> TokenOfOwnerByIndexERC721(string address, string token, BigInteger index, bool parent = false) {
            var web3Object = parent ? this.ParentWeb3() : this.ChildWeb3();
            var contract = this._GetERC721TokenContract(token, web3Object);
            var tokenId = await contract.GetFunction("tokenOfOwnerByIndex").CallAsync<BigInteger>(address, index);
            return tokenId;
        }

        // deposit

        public async Task ApproveERC20TokensForDeposit(string from, string token, BigInteger amount) {
            // create token contract
            var _tokenContract = this.ParentWeb3().Eth.GetContract(
                this.StandardTokenArtifacts,
                token
            );
            var function = _tokenContract.GetFunction("approve");
            await this._SendTransaction(this.ParentWeb3(), function, from, null, this._rootChainAddress, amount);
        }

        public async Task DepositERC20Tokens(string from, string user, string token, BigInteger amount) {
            var function = this._rootChainContract.GetFunction("deposit");
            await this._SendTransaction(this.ParentWeb3(), function, from, null, token, user, amount);
        }

        public async Task SafeDepositERC721Tokens(string from, string token, BigInteger tokenId) {
            var _tokenContract = this._GetERC721TokenContract(token, this.ParentWeb3());
            var function = _tokenContract.GetFunction("safeTransferFrom");
            await this._SendTransaction(this.ParentWeb3(), function, from, null, from, this._rootChainAddress, tokenId);
        }

        public async Task ApproveERC721TokensForDeposit(string from, string token, BigInteger tokenId) {
            // create token contract
            var _tokenContract = this._GetERC721TokenContract(token, this.ParentWeb3());
            var function = _tokenContract.GetFunction("approve");
            await this._SendTransaction(this.ParentWeb3(), function, from, null, this._rootChainAddress, tokenId);
        }

        public async Task DepositERC721Tokens(string from, string user, string token, BigInteger tokenId) {
            var function = this._rootChainContract.GetFunction("depositERC721");
            await this._SendTransaction(this.ParentWeb3(), function, from, null, token, user, tokenId);
        }

        public async Task DepositEthers(string from, BigInteger value) {
            var function = this._rootChainContract.GetFunction("depositEthers");
            await this._SendTransaction(this.ParentWeb3(), function, from, new HexBigInteger(value));
        }

        // transfer

        public async Task<string> TransferTokens(string from, string token, string user, BigInteger amount, bool parent = false) {
            var web3Object = parent ? this.ParentWeb3() : this.ChildWeb3();
            var contract = this._GetERC20TokenContract(token, web3Object);
            var function = contract.GetFunction("transfer");
            return await this._SendTransaction(web3Object, function, from, null, user, amount);
        }

        public async Task<string> TransferERC721Tokens(string from, string token, string user, BigInteger tokenId, bool parent = false) {
            var web3Object = parent ? this.ParentWeb3() : this.ChildWeb3();
            var contract = this._GetERC721TokenContract(token, web3Object);
            var function = contract.GetFunction("transferFrom");
            return await this._SendTransaction(web3Object, function, from, null, from, user, tokenId);
        }

        public async Task<string> TransferEthers(string from, string to, BigInteger amount, bool parent = false, bool isCutomEth = false) {
            // if matic chain, transfer normal WETH tokens
            if (!parent && !isCutomEth) {
                return await this.TransferTokens(from, this._maticWethAddress, to, amount, parent);
            }
            var web3Object = (!parent && isCutomEth) ? this.ChildWeb3() : this.ParentWeb3();
            // transfer Eth
            var amountDecimal  = Nethereum.Web3.Web3.Convert.FromWei(amount);
            var gasPrice = await web3Object.Eth.GasPrice.SendRequestAsync().ConfigureAwait(false);
            var gasPriceDecimal = Nethereum.Web3.Web3.Convert.FromWei(gasPrice.Value, UnitConversion.EthUnit.Gwei);
            var transferService = web3Object.Eth.GetEtherTransferService();
            var gas = await transferService.EstimateGasAsync(to, amountDecimal);
            var receiptTxn = await transferService
                .TransferEtherAndWaitForReceiptAsync(to, amountDecimal, gasPriceDecimal, gas);
            return receiptTxn.TransactionHash;
        }

        // withdraw

        public async Task<string> StartWithdraw(string from, string token, BigInteger amount) {
            var web3Object = this.ChildWeb3();
            var contract = this._GetERC20TokenContract(token, web3Object);
            var function = contract.GetFunction("withdraw");
            return await this._SendTransaction(web3Object, function, from, null, amount);
        }
        public async Task<string> StartERC721Withdraw(string from, string token, BigInteger tokenId) {
            var web3Object = this.ChildWeb3();
            var contract = this._GetERC721TokenContract(token, web3Object);
            var function = contract.GetFunction("withdraw");
            return await this._SendTransaction(web3Object, function, from, null, tokenId);
        }

        public async Task<string> Withdraw(string from, string txId) {
            // fetch trancation & receipt proof
            var txProof = await this.GetTxProof(txId);
            var receiptProof = await this.GetReceiptProof(txId);

            // fetch header object & header proof
            JToken header = null;
            try {
                header = await this.GetHeaderObject(txProof["blockNumber"].ToString());
            } catch (Exception) {
                // ignore error
            }
            // check if header block found
            if (header == null){
                throw new Exception($"No corresponding checkpoint / header block found for { txId}.");
            }

            var headerProof = await this.GetHeaderProof(txProof["blockNumber"].ToString(), header["start"].ToString(), header["end"].ToString());
            // build proof
            var hashes = headerProof["proof"].ToObject<List<string>>();
            var proofBuffer = new byte[]{};
            foreach (string hash in hashes) {
                byte[] hashBuffer = hash.HexToByteArray();
                proofBuffer = proofBuffer.Concat(hashBuffer).ToArray();
            }
            // Console.WriteLine($"proof buffer: {proofBuffer.ToHex(prefix: true)}");

            var web3Object = this.ParentWeb3();
            var contract = this._withdrawManagerContract;
            var function = contract.GetFunction("withdrawBurntTokens");
            return await this._SendTransaction(web3Object, function, from, null,
                header["number"].ToString(), // header block
                proofBuffer, //.ToHex(prefix: true), // header proof
                txProof["blockNumber"].ToString(), // block number
                txProof["blockTimestamp"].ToString(), // block timestamp
                txProof["root"].ToString().HexToByteArray(), // tx root
                receiptProof["root"].ToString().HexToByteArray(), // receipt root
                Nethereum.RLP.RLP.EncodeElement(receiptProof["path"].ToString().HexToByteArray()), // .ToHex(prefix: true); // key for trie (both tx and receipt)
                txProof["value"].ToString().HexToByteArray(), // tx bytes
                txProof["parentNodes"].ToString().HexToByteArray(), // tx proof nodes
                receiptProof["value"].ToString().HexToByteArray(), // receipt bytes
                receiptProof["parentNodes"].ToString().HexToByteArray() // reciept proof nodes
            );
        }

        public async Task<string> ProcessExits(string from, string rootTokenAddress) {
            var web3Object = this.ParentWeb3();
            var contract = this._withdrawManagerContract;
            var function = contract.GetFunction("processExits");
            return await this._SendTransaction(web3Object, function, from, null, rootTokenAddress);
        }

        // transaction

        public async Task<Transaction> GetTx(string txId) {
            if (this._syncerUrl != null) {
                try {
                    var response = await this._ApiCall($"{this._syncerUrl}/tx/{txId}");
                    if (response != null && response["tx"] != null) {
                        return response["tx"].ToObject<Transaction>();
                    }
                } catch(Exception) {
                    // ignore error
                }
            }
            return await this.ChildWeb3().Eth.Transactions.GetTransactionByHash.SendRequestAsync(txId);
        }

        private async Task<JToken> GetTxProof(string txId)
        {
            try
            {
                var response = await this._ApiCall($"{this._syncerUrl}/tx/{txId}/proof");
                if (response != null && response["proof"] != null)
                {
                    return response["proof"];
                }
            }
            catch (Exception)
            {
                // ignore error
            }
            return null;
        }

        public async Task<TransactionReceipt> GetReceipt(string txId) {
            if (this._syncerUrl != null) {
                try {
                    var response = await this._ApiCall($"{this._syncerUrl}/tx/{txId}/receipt");
                    if (response != null && response["receipt"] != null) {
                        return response["receipt"].ToObject<TransactionReceipt>();
                    }
                } catch (Exception) {
                    // ignore error
                }
            }
            return await this.ChildWeb3().Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
        }

        private async Task<JToken> GetReceiptProof(string txId) {
            try {
                var response = await this._ApiCall($"{this._syncerUrl}/tx/{txId}/receipt/proof");
                if (response != null && response["proof"] != null) {
                    return response["proof"];
                }
            } catch (Exception) {
                // ignore error
            }
            return null;
        }

        public async Task<JToken> GetHeaderObject(string blockNumber) {
            try {
                var response = await this._ApiCall($"{this._watcherUrl}/header/included/{blockNumber}");
                if (response != null) {
                    return response;
                }
            } catch (Exception) {
                // ignore error
            }
            return null;
        }

        public async Task<JToken> GetHeaderProof(string blockNumber, string start, string end)
        {
            try {
                var response = await this._ApiCall($"{this._syncerUrl}/block/{blockNumber}/proof", new Dictionary<string, string> {
                    { "start", start } ,
                    { "end", end }
                });
                if (response != null && response["proof"] != null) {
                    return response["proof"];
                }
            } catch (Exception) {
                // ignore error
            }
            return null;
        }


        // utilities
        private async Task<string> _SendTransaction(IWeb3 web3Object, Function function, string from, HexBigInteger value, params object[] functionInput) {
            // get gas price
            var gasPrice = await web3Object.Eth.GasPrice.SendRequestAsync().ConfigureAwait(false);
            gasPrice = gasPrice.ToUlong() == 0 ? null : gasPrice;
            // Console.WriteLine($"gasPrice: {gasPrice}");
            // estimate gas
            // var gas = new HexBigInteger(170000);
            var gas = await function.EstimateGasAsync(from, null, value, functionInput);
            // Console.WriteLine($"gas: {gas}");
            // send transaction
            var receiptTxn = await function.SendTransactionAndWaitForReceiptAsync(from, gas, gasPrice, value, null, functionInput);
            // var hash = await function.SendTransactionAsync(from, gas, gasPrice, value, functionInput);
            return receiptTxn.TransactionHash;
        }

        private Contract _GetERC721TokenContract(string token, IWeb3 web3Object)
        {
            var _token = token.ToLower();
            if (!this._tokenCache.ContainsKey(_token))
            {
                var _tokenContract = web3Object.Eth.GetContract(ChildERC721Artifacts, _token);
                // update token cache
                this._tokenCache.Add(_token, _tokenContract);
            }
            return this._tokenCache[_token];
        }

        private Contract _GetERC20TokenContract(string token, IWeb3 web3Object)
        {
            var _token = token.ToLower();
            if (!this._tokenCache.ContainsKey(_token))
            {
                var _tokenContract = web3Object.Eth.GetContract(ChildERC20Artifacts, _token);
                // update token cache
                this._tokenCache.Add(_token, _tokenContract);
            }
            return this._tokenCache[_token];
        }

       private async Task<JObject> _ApiCall(string url, Dictionary<string, string> queryParams = null, Dictionary<string, string> body = null) {
            webClient.DefaultRequestHeaders.Add("Accept", "application/json");
            webClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            if (queryParams != null) {
                var content = new FormUrlEncodedContent(queryParams);
                url = url + "?" + content.ReadAsStringAsync().Result;
            }

            Console.WriteLine($"send request: {url}");

            string responseString = null;
            if (body == null) {
                responseString = await webClient.GetStringAsync(url);
            } else {
                var content = new FormUrlEncodedContent(body);
                var response = await webClient.PostAsync(url, content);
                responseString = await response.Content.ReadAsStringAsync();
            }

            if (responseString != null) {
                // return JsonConvert.DeserializeObject(responseString);
                return JObject.Parse(responseString);
            } else {
                return null;
            }

       }

    }
}
