using System.Numerics;
using Nethereum.Hex.HexTypes;
using System.Threading.Tasks;
using Nethereum.RPC.Eth.DTOs;
using Newtonsoft.Json.Linq;

namespace MaticNetwork.Net
{
    public interface IMatic
    {
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

        Task<Transaction> GetTx(string txId);
        Task<TransactionReceipt> GetReceipt(string txId);
        Task<JToken> GetHeaderObject(string blockNumber);
    }
}
