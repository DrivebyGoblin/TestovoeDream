using System;
using System.Collections.Generic;
using UnityEngine;

public class FactoryModel
{
    private readonly List<MachineModel> _machines;
    private readonly WalletModel _wallet;
    private readonly BoostService _boostService;

    public event Action OnUpdated;

    public FactoryModel(List<MachineModel> machines, WalletModel wallet, BoostService boostService)
    {
        _machines = machines ?? new List<MachineModel>();
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
        _boostService = boostService ?? throw new ArgumentNullException(nameof(boostService));

        // Пересчитываем доход и UI, когда буст включился или закончился
        _boostService.OnBoostStateChanged += HandleBoostStateChanged;
    }

    private void HandleBoostStateChanged()
    {
        OnUpdated?.Invoke();
    }

    public double GetTotalProduction()
    {
        double total = 0;
        for (int i = 0; i < _machines.Count; i++)
        {
            total += _machines[i].GetCurrentProduction();
        }

        // Автоматически применяет multiplier, если буст активен, иначе 1f
        return total * _boostService.CurrentMultiplier;
    }

    public void Tick(float deltaTime)
    {
        double income = GetTotalProduction() * deltaTime;
        if (income > 0)
        {
            _wallet.Add(income);
        }
    }
}
