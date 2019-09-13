using System.Numerics;
using Nethereum.Hex.HexTypes;
using System.Threading.Tasks;

namespace MaticNetwork.Net
{
    public interface IMatic
    {
        // API references: https://docs.matic.network/getting-started/

        void SetPrivateKey(string privateKey);

        Task<string> GetMappedTokenAddress(string tokenAddress);
        Task<BigInteger> BalanceOfERC721(string address, string token, bool parent = false);
        Task<BigInteger> BalanceOfERC20(string address, string token, bool parent = false);
        Task<string> TokenOfOwnerByIndexERC721(string address, string token, int index, bool parent = false);

        // deposit

        Task ApproveERC20TokensForDeposit(string from, string token, BigInteger amount);
        Task DepositERC20Tokens(string from, string user, string token, BigInteger amount);
        Task SafeDepositERC721Tokens(string from, string token, string tokenId);
        Task ApproveERC721TokensForDeposit(string from, string token, string tokenId);
        Task DepositERC721Tokens(string from, string user, string token, string tokenId);
        Task DepositEthers(string from, string value);

        // transfer

        // matic.transferTokens()
        // matic.transferERC721Tokens()
        // matic.transferEthers()

        // withdraw

        // matic.startWithdraw()
        // matic.startERC721Withdraw()
        // matic.withdraw()
        // matic.processExits()

        // read info

        // matic.getTx()
        // matic.getReceipt()
        // matic.getHeaderObject()
    }
}
