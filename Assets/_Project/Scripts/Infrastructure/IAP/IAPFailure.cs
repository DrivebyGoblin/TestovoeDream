using UnityEngine;

public readonly struct IAPFailure
{
    public string ProductId { get; }
    public string Message { get; }

    public IAPFailure(string productId, string message)
    {
        ProductId = productId;
        Message = message;
    }
}