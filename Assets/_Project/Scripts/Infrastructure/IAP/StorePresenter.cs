using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class StorePresenter : IDisposable
{
    private readonly HeaderView _view;
    private readonly WalletModel _wallet;
    private readonly IIAPService _iapService;
    private readonly PurchaseHistory _purchaseHistory;
    private readonly Action _saveProgress;
    private readonly string _productId;
    private readonly double _reward;

    private bool _purchaseInProgress;

    public StorePresenter(HeaderView view, WalletModel wallet, IIAPService iapService, PurchaseHistory purchaseHistory, Action saveProgress, string productId, double reward)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        _iapService = iapService ?? throw new ArgumentNullException(nameof(iapService));
        _purchaseHistory = purchaseHistory ?? throw new ArgumentNullException(nameof(purchaseHistory));
        _saveProgress = saveProgress ?? throw new ArgumentNullException(nameof(saveProgress));
        _productId = string.IsNullOrWhiteSpace(productId) ? throw new ArgumentException("Product ID cannot be empty.", nameof(productId)) : productId;
        _reward = reward > 0 ? reward : throw new ArgumentOutOfRangeException(nameof(reward), "Purchase reward must be positive.");

        _view.OnPurchaseClicked += HandlePurchaseClicked;
        _iapService.OnProductReceived += HandleProductReceived;
        _iapService.OnPurchasePending += HandlePurchasePending;
        _iapService.OnPurchaseFailed += HandlePurchaseFailed;
        _iapService.OnPurchaseDeferred += HandlePurchaseDeferred;
        _iapService.OnUnavailable += HandleUnavailable;

        _view.SetStoreFeatureActive(true);
        _view.SetStoreReward(_reward);
        _view.SetStorePrice(string.Empty);
        _view.SetStoreStatus("Connecting to store...");
        _view.SetPurchaseInteractable(false);
    }

    public Task InitializeAsync()
    {
        return _iapService.InitializeAsync(_productId);
    }

    public void Dispose()
    {
        _view.OnPurchaseClicked -= HandlePurchaseClicked;
        _iapService.OnProductReceived -= HandleProductReceived;
        _iapService.OnPurchasePending -= HandlePurchasePending;
        _iapService.OnPurchaseFailed -= HandlePurchaseFailed;
        _iapService.OnPurchaseDeferred -= HandlePurchaseDeferred;
        _iapService.OnUnavailable -= HandleUnavailable;
    }

    private void HandlePurchaseClicked()
    {
        if (_purchaseInProgress || !_iapService.IsInitialized)
        {
            return;
        }

        _purchaseInProgress = true;
        _view.SetPurchaseInteractable(false);
        _view.SetStoreStatus("Processing purchase...");
        _iapService.BuyProduct(_productId);
    }

    private void HandleProductReceived(IAPProduct product)
    {
        if (product.Id != _productId)
        {
            return;
        }

        _view.SetStorePrice(product.LocalizedPrice);
        _view.SetStoreStatus("Ready");
        _view.SetPurchaseInteractable(true);
    }

    private void HandlePurchasePending(IAPPurchase purchase)
    {
        if (purchase.ProductId != _productId)
        {
            return;
        }

        try
        {
            if (!_purchaseHistory.IsProcessed(purchase.TransactionId))
            {
                _wallet.Add(_reward);
                _purchaseHistory.MarkProcessed(purchase.TransactionId);
                _saveProgress();
                AnalyticsEvents.LogPurchaseSucceeded(purchase.ProductId, _reward, purchase.TransactionId);
            }

            _iapService.ConfirmPurchase(purchase.TransactionId);
            _view.SetStoreStatus($"Purchase complete: +{_reward:F0} coins");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            _view.SetStoreStatus("Could not save the purchase");
        }
        finally
        {
            _purchaseInProgress = false;
            _view.SetPurchaseInteractable(_iapService.IsInitialized);
        }
    }

    private void HandlePurchaseFailed(IAPFailure failure)
    {
        if (!string.IsNullOrEmpty(failure.ProductId) && failure.ProductId != _productId)
        {
            return;
        }

        _purchaseInProgress = false;
        AnalyticsEvents.LogPurchaseFailed(string.IsNullOrEmpty(failure.ProductId) ? _productId : failure.ProductId, failure.Message);
        _view.SetStoreStatus($"Purchase failed: {failure.Message}");
        _view.SetPurchaseInteractable(_iapService.IsInitialized);
    }

    private void HandlePurchaseDeferred(string productId)
    {
        if (productId != _productId)
        {
            return;
        }

        _purchaseInProgress = false;
        _view.SetStoreStatus("Purchase is awaiting approval");
        _view.SetPurchaseInteractable(_iapService.IsInitialized);
    }

    private void HandleUnavailable(string message)
    {
        _purchaseInProgress = false;
        _view.SetStoreStatus(message);
        _view.SetPurchaseInteractable(false);
    }
}