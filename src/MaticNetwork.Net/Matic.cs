using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.Contracts;

namespace MaticNetwork.Net
{
    public class Matic: IMatic
    {
        private IWeb3 _web3;
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

        public Matic (string maticProvider, string parentProvider, string syncerUrl = null, string watcherUrl = null, string rootChainAddress = null, string maticWethAddress = null, string withdrawManagerAddress = null, string depositManagerAddress = null) {

            this._web3 = new Web3(maticProvider);
            // this._web3.matic = true
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

            // create rootchain contract
            this._rootChainContract = this._parentWeb3.Eth.GetContract(
                this.RootChainArtifacts,
                this._rootChainAddress
            );

            // create withdraw manager contract
            this._withdrawManagerContract = this._parentWeb3.Eth.GetContract(
                this.WithdrawManagerArtifacts,
                this._withdrawManagerAddress
            );

            // create deposit manager contract
            this._depositManagerContract = this._parentWeb3.Eth.GetContract(
                this.DepositManagerArtifacts,
                this._depositManagerAddress
            );
        }

        //
        // Getters & setters
        //

        public IWeb3 web3()
        {
            return this._web3;
        }

        public IWeb3 parentWeb3()
        {
            return this._parentWeb3;
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
            var web3Object = parent ? this._parentWeb3 : this._web3;
            var contract = this._GetERC721TokenContract(token, web3Object);
            var balance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(address);
            return balance;
        }

        public async Task<BigInteger> BalanceOfERC20(string address, string token, bool parent = false)
        {
            var web3Object = parent ? this._parentWeb3 : this._web3;
            var contract = this._GetERC20TokenContract(token, web3Object);
            var balance = await contract.GetFunction("balanceOf").CallAsync<BigInteger>(address);
            return balance;
        }

        public async Task<string> TokenOfOwnerByIndexERC721(string address, string token, int index, bool parent = false) {
            var web3Object = parent ? this._parentWeb3 : this._web3;
            var contract = this._GetERC721TokenContract(token, web3Object);
            var tokenId = await contract.GetFunction("tokenOfOwnerByIndex").CallAsync<string>(address, index);
            return tokenId;
        }

        public async void depositEther(string from, BigInteger value) {
            var depositTx = this._rootChainContract.GetFunction("depositEthers");
            // var _options = await this._fillOptions(
            //     options,
            //     depositTx,
            //     this._parentWeb3
            // )
            await depositTx.SendTransactionAsync(from, value);
        }

        private Contract _GetERC721TokenContract(string token, IWeb3 web3)
        {
            var _token = token.ToLower();
            if (!this._tokenCache.ContainsKey(_token))
            {
                var _tokenContract = web3.Eth.GetContract(ChildERC721Artifacts, _token);
                // update token cache
                this._tokenCache.Add(_token, _tokenContract);
            }
            return this._tokenCache[_token];
        }

        private Contract _GetERC20TokenContract(string token, IWeb3 web3)
        {
            var _token = token.ToLower();
            if (!this._tokenCache.ContainsKey(_token))
            {
                var _tokenContract = web3.Eth.GetContract(ChildERC20Artifacts, _token);
                // update token cache
                this._tokenCache.Add(_token, _tokenContract);
            }
            return this._tokenCache[_token];
        }

    }
}
