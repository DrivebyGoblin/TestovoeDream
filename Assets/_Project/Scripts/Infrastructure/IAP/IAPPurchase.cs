using UnityEngine;

public readonly struct IAPPurchase
{
    public string ProductId { get; }
    public string TransactionId { get; }

    public IAPPurchase(string productId, string transactionId)
    {
        ProductId = productId;
        TransactionId = transactionId;
    }
}