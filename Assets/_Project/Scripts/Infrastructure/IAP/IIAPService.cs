using System;
using System.Threading.Tasks;

public interface IIAPService : IDisposable
{
    bool IsInitialized { get; }

    event Action<IAPProduct> OnProductReceived;
    event Action<IAPPurchase> OnPurchasePending;
    event Action<IAPFailure> OnPurchaseFailed;
    event Action<string> OnPurchaseDeferred;
    event Action<string> OnUnavailable;

    Task InitializeAsync(string productId);
    void BuyProduct(string productId);
    void ConfirmPurchase(string transactionId);
}





