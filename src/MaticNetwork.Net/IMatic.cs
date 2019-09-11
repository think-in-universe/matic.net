using System.Numerics;
using System.Threading.Tasks;

namespace MaticNetwork.Net
{
    public interface IMatic
    {
        // API references: https://docs.matic.network/getting-started/

        // new Matic()
        Task<string> GetMappedTokenAddress(string tokenAddress);
        Task<BigInteger> BalanceOfERC721(string address, string token, bool parent = false);
        Task<BigInteger> BalanceOfERC20(string address, string token, bool parent = false);
        Task<string> TokenOfOwnerByIndexERC721(string address, string token, int index, bool parent = false);
        void depositEther(string from, BigInteger value);
        // matic.approveERC20TokensForDeposit()
        // matic.depositERC20Tokens()
        // matic.safeTransferFrom()
        // matic.approveERC721TokenForDeposit()
        // matic.depositERC721Tokens()
        // matic.depositEthers()
        // matic.transferTokens()
        // matic.transferERC721Tokens()
        // matic.transferEthers()
        // matic.startWithdraw()
        // matic.startERC721Withdraw()
        // matic.getHeaderObject()
        // matic.withdraw()
        // matic.processExits()
        // matic.getTx()
        // matic.getReceipt()
    }
}
