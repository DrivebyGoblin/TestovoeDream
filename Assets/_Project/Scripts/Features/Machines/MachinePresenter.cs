using System;
using UnityEngine;

public class MachinePresenter : IDisposable
{
    private readonly MachineView _view;
    private readonly MachineModel _model;
    private readonly WalletModel _wallet;

    public MachinePresenter(MachineView view, MachineModel model, WalletModel wallet)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));

        _model.OnUpdated += HandleModelUpdated;
        _wallet.OnBalanceChanged += HandleBalanceChanged;

        _view.OnUnlockClicked += HandleUnlockClicked;
        _view.OnUpgradeClicked += HandleUpgradeClicked;

        InitializeView();
    }

    public void Dispose()
    {
        _model.OnUpdated -= HandleModelUpdated;
        _wallet.OnBalanceChanged -= HandleBalanceChanged;

        _view.OnUnlockClicked -= HandleUnlockClicked;
        _view.OnUpgradeClicked -= HandleUpgradeClicked;
    }

    private void InitializeView()
    {
        _view.SetTitle(_model.Config.Title);
        UpdateViewData();
    }

    private void HandleModelUpdated()
    {
        UpdateViewData();
    }

    private void HandleBalanceChanged(double newBalance)
    {
        UpdateInteractableState();
    }

    private void HandleUnlockClicked()
    {
        if (_model.State != EMachineState.Locked) return;

        double cost = _model.GetUnlockCost();
        if (_wallet.TrySpend(cost))
        {
            _model.Unlock();
            AnalyticsEvents.LogMachineUnlocked(_model.Config.Id, cost);
        }
    }

    private void HandleUpgradeClicked()
    {
        if (_model.State != EMachineState.Unlocked) return;

        double cost = _model.GetCurrentUpgradeCost();
        if (_wallet.TrySpend(cost))
        {
            _model.LevelUp();
            AnalyticsEvents.LogMachineUpgraded(_model.Config.Id, _model.Level, cost);
        }
    }

    private void UpdateViewData()
    {
        bool isUnlocked = _model.State == EMachineState.Unlocked;

        // 1. Включаем нужную панель (Unlocked или Locked)
        _view.SetState(isUnlocked);
        _view.SetStateText(isUnlocked ? "Unlocked" : "Locked");

        // 2. Обновляем тексты
        int level = _model.Level;
        double production = _model.GetCurrentProduction();
        double upgradeCost = _model.GetCurrentUpgradeCost();
        double unlockCost = _model.GetUnlockCost();

        _view.UpdateView(level, production, upgradeCost, unlockCost);

        // 3. Обновляем кликабельность
        UpdateInteractableState();
    }

    private void UpdateInteractableState()
    {
        bool isUnlocked = _model.State == EMachineState.Unlocked;

        // Если машина закрыта — проверяем цену Unlock
        // Если машина открыта — кнопка Unlock вообще не нужна, а Upgrade проверяем по балансу
        bool canUnlock = !isUnlocked && _wallet.CanAfford(_model.GetUnlockCost());
        bool canUpgrade = isUnlocked && _wallet.CanAfford(_model.GetCurrentUpgradeCost());

        _view.SetInteractable(canUnlock, canUpgrade);
    }

}
