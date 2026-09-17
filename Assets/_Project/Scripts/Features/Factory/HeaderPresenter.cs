using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class HeaderPresenter : IDisposable
{
    private readonly HeaderView _view;
    private readonly FactoryModel _factoryModel;
    private readonly WalletModel _wallet;
    private readonly BoostService _boostService;

    public HeaderPresenter(HeaderView view, FactoryModel factoryModel, WalletModel wallet, BoostService boostService)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _factoryModel = factoryModel ?? throw new ArgumentNullException(nameof(factoryModel));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        _boostService = boostService ?? throw new ArgumentNullException(nameof(boostService));

        _wallet.OnBalanceChanged += HandleBalanceChanged;
        _factoryModel.OnUpdated += HandleFactoryUpdated;
        _boostService.OnBoostStateChanged += HandleBoostStateChanged;
        _view.OnBoostClicked += HandleBoostClicked;

        _view.SetBoostFeatureActive(_boostService.IsFeatureEnabled);
        UpdateView();
    }

    public void Dispose()
    {
        _wallet.OnBalanceChanged -= HandleBalanceChanged;
        _factoryModel.OnUpdated -= HandleFactoryUpdated;
        _boostService.OnBoostStateChanged -= HandleBoostStateChanged;
        _view.OnBoostClicked -= HandleBoostClicked;
    }

    private void HandleBoostClicked()
    {
        if (!_boostService.IsActive)
        {
            _boostService.ActivateBoost();
        }
    }

    private void HandleBoostStateChanged() => UpdateView();
    private void HandleBalanceChanged(double newBalance) => UpdateView();
    private void HandleFactoryUpdated() => UpdateView();

    private void UpdateView()
    {
        _view.UpdateEconomy(_wallet.Balance, _factoryModel.GetTotalProduction());
        _view.SetBoostState(_boostService.IsActive);
    }
}


