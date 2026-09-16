using System;
using UnityEngine;

public class WalletModel
{
    public double Balance { get; private set; }
    public event Action<double> OnBalanceChanged;

    public WalletModel(double initialBalance = 0)
    {
        Balance = initialBalance;
    }

    public void Add(double amount)
    {
        if (amount <= 0) return;
        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);
    }

    public bool CanAfford(double amount) => Balance >= amount;

    public bool TrySpend(double amount)
    {
        if (!CanAfford(amount)) return false;

        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }
}
