using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

public sealed class UnityIAPService : IIAPService
{
    private readonly Dictionary<string, PendingOrder> _pendingOrders = new Dictionary<string, PendingOrder>();

    private StoreController _storeController;
    private string _productId;
    private Product _product;
    private bool _isInitializing;
    private bool _isDisposed;

    public bool IsInitialized { get; private set; }

    public event Action<IAPProduct> OnProductReceived;
    public event Action<IAPPurchase> OnPurchasePending;
    public event Action<IAPFailure> OnPurchaseFailed;
    public event Action<string> OnPurchaseDeferred;
    public event Action<string> OnUnavailable;

    public async Task InitializeAsync(string productId)
    {
        if (_isDisposed || IsInitialized || _isInitializing)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            ReportUnavailable("IAP product ID is empty.");
            return;
        }

        _isInitializing = true;
        _productId = productId;
        _storeController = UnityIAPServices.StoreController();
        SubscribeToStoreEvents();

        try
        {
            await _storeController.Connect();

            if (_isDisposed)
            {
                return;
            }

            _storeController.FetchProducts(new List<ProductDefinition>
            {
                new ProductDefinition(_productId, ProductType.Consumable)
            });
        }
        catch (Exception exception)
        {
            ReportUnavailable($"IAP initialization failed: {exception.Message}");
        }
        finally
        {
            _isInitializing = false;
        }
    }

    public void BuyProduct(string productId)
    {
        if (!IsInitialized || _storeController == null)
        {
            ReportPurchaseFailure(productId, "IAP is not initialized.");
            return;
        }

        if (_product == null || _product.definition.id != productId || !_product.availableToPurchase)
        {
            ReportPurchaseFailure(productId, "The product is unavailable.");
            return;
        }

        _storeController.PurchaseProduct(_product);
    }

    public void ConfirmPurchase(string transactionId)
    {
        if (_storeController == null || string.IsNullOrEmpty(transactionId))
        {
            ReportPurchaseFailure(_productId, "The purchase cannot be confirmed.");
            return;
        }

        if (!_pendingOrders.Remove(transactionId, out PendingOrder order))
        {
            ReportPurchaseFailure(_productId, "The pending purchase was not found.");
            return;
        }

        _storeController.ConfirmPurchase(order);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        IsInitialized = false;
        UnsubscribeFromStoreEvents();
        _pendingOrders.Clear();
        _product = null;
        _storeController = null;
    }

    private void SubscribeToStoreEvents()
    {
        _storeController.OnStoreDisconnected += HandleStoreDisconnected;
        _storeController.OnProductsFetched += HandleProductsFetched;
        _storeController.OnProductsFetchFailed += HandleProductsFetchFailed;
        _storeController.OnPurchasePending += HandlePurchasePending;
        _storeController.OnPurchaseConfirmed += HandlePurchaseConfirmed;
        _storeController.OnPurchaseFailed += HandlePurchaseFailed;
        _storeController.OnPurchaseDeferred += HandlePurchaseDeferred;
    }

    private void UnsubscribeFromStoreEvents()
    {
        if (_storeController == null)
        {
            return;
        }

        _storeController.OnStoreDisconnected -= HandleStoreDisconnected;
        _storeController.OnProductsFetched -= HandleProductsFetched;
        _storeController.OnProductsFetchFailed -= HandleProductsFetchFailed;
        _storeController.OnPurchasePending -= HandlePurchasePending;
        _storeController.OnPurchaseConfirmed -= HandlePurchaseConfirmed;
        _storeController.OnPurchaseFailed -= HandlePurchaseFailed;
        _storeController.OnPurchaseDeferred -= HandlePurchaseDeferred;
    }

    private void HandleProductsFetched(List<Product> products)
    {
        _product = products.FirstOrDefault(product => product.definition.id == _productId);

        if (_product == null || !_product.availableToPurchase)
        {
            ReportUnavailable($"Product '{_productId}' is unavailable.");
            return;
        }

        IsInitialized = true;
        string localizedPrice = _product.metadata?.localizedPriceString ?? string.Empty;
        OnProductReceived?.Invoke(new IAPProduct(_productId, localizedPrice));
    }

    private void HandleProductsFetchFailed(ProductFetchFailed failure)
    {
        ReportUnavailable($"Failed to load IAP products: {failure.FailureReason}");
    }

    private void HandlePurchasePending(PendingOrder order)
    {
        CartItem item = order.CartOrdered.Items().FirstOrDefault();
        string productId = item?.Product?.definition?.id;
        string transactionId = order.Info?.TransactionID;

        if (string.IsNullOrEmpty(productId) || string.IsNullOrEmpty(transactionId))
        {
            ReportPurchaseFailure(productId, "The store returned an invalid purchase.");
            return;
        }

        _pendingOrders[transactionId] = order;
        OnPurchasePending?.Invoke(new IAPPurchase(productId, transactionId));
    }

    private void HandlePurchaseConfirmed(Order order)
    {
        if (order is FailedOrder failedOrder)
        {
            HandlePurchaseFailed(failedOrder);
        }
    }

    private void HandlePurchaseFailed(FailedOrder order)
    {
        string productId = order.CartOrdered.Items().FirstOrDefault()?.Product?.definition?.id;
        string message = $"{order.FailureReason}: {order.Details}";
        ReportPurchaseFailure(productId, message);
    }

    private void HandlePurchaseDeferred(DeferredOrder order)
    {
        string productId = order.CartOrdered.Items().FirstOrDefault()?.Product?.definition?.id;
        OnPurchaseDeferred?.Invoke(productId);
    }

    private void HandleStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        ReportUnavailable($"IAP store is unavailable: {failure.Message}");
    }

    private void ReportPurchaseFailure(string productId, string message)
    {
        Debug.LogWarning($"[IAP] Purchase failed. Product: {productId}. {message}");
        OnPurchaseFailed?.Invoke(new IAPFailure(productId, message));
    }

    private void ReportUnavailable(string message)
    {
        IsInitialized = false;
        Debug.LogWarning($"[IAP] {message}");
        OnUnavailable?.Invoke(message);
    }
}
