using System;
using NUnit.Framework;

public sealed class MiniFactoryLogicTests
{
    private static readonly DateTime ExitTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void OfflineIncome_BoostExpiresDuringAbsence_OnlyBoostsItsRemainingDuration()
    {
        var service = new OfflineProgressService();

        var result = service.Calculate(ExitTime, ExitTime.AddSeconds(100),
            maxOfflineDuration: 300, remainingBoostTime: 30,
            boostMultiplier: 2, baseProductionPerSecond: 10);

        // 30 seconds at 20 coins/s, then 70 seconds at 10 coins/s.
        Assert.That(result.EarnedCoins, Is.EqualTo(1300).Within(0.001));
        Assert.That(result.UsedBoostSeconds, Is.EqualTo(30));
        Assert.That(result.ElapsedSeconds, Is.EqualTo(100));
    }

    [Test]
    public void OfflineIncome_AbsenceExceedsCap_DoesNotPayBeyondCap()
    {
        var service = new OfflineProgressService();

        var result = service.Calculate(ExitTime, ExitTime.AddHours(24),
            maxOfflineDuration: 60, remainingBoostTime: 120,
            boostMultiplier: 3, baseProductionPerSecond: 5);

        Assert.That(result.EarnedCoins, Is.EqualTo(900).Within(0.001));
        Assert.That(result.ElapsedSeconds, Is.EqualTo(60));
        Assert.That(result.UsedBoostSeconds, Is.EqualTo(60));
    }

    [Test]
    public void OfflineIncome_ClockMovesBackwards_DoesNotGrantIncomeOrConsumeBoost()
    {
        var service = new OfflineProgressService();

        var result = service.Calculate(ExitTime, ExitTime.AddHours(-1),
            maxOfflineDuration: 300, remainingBoostTime: 30,
            boostMultiplier: 2, baseProductionPerSecond: 10);

        Assert.That(result.EarnedCoins, Is.Zero);
        Assert.That(result.ElapsedSeconds, Is.Zero);
        Assert.That(result.UsedBoostSeconds, Is.Zero);
    }

    [Test]
    public void Wallet_InsufficientFunds_RejectsPaymentWithoutChangingBalanceOrNotifying()
    {
        var wallet = new WalletModel(50);
        int notifications = 0;
        wallet.OnBalanceChanged += _ => notifications++;

        bool paid = wallet.TrySpend(75);

        Assert.That(paid, Is.False);
        Assert.That(wallet.Balance, Is.EqualTo(50));
        Assert.That(notifications, Is.Zero);
    }

    [Test]
    public void PurchaseHistory_SaveAndRestore_PreservesProcessedTransactionWithoutDuplicates()
    {
        var original = new PurchaseHistory();
        original.MarkProcessed("transaction-1");
        original.MarkProcessed("transaction-1");
        var snapshot = original.CreateSnapshot();
        var restored = new PurchaseHistory();

        restored.Restore(snapshot);
        snapshot.Clear();

        Assert.That(restored.IsProcessed("transaction-1"), Is.True);
        Assert.That(restored.IsProcessed("transaction-2"), Is.False);
        Assert.That(restored.CreateSnapshot(), Has.Count.EqualTo(1));
    }
}
