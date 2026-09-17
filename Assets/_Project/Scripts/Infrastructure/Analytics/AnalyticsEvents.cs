using System.Collections.Generic;
using UnityEngine;

public static class AnalyticsEvents
{
    private static AnalyticsFacade _manager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        _manager = null;
    }

    private static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (_manager == null)
        {
            Debug.LogWarning($"[Analytics] Initialize analytics before sending {eventName}.");
            return;
        }

        _manager.LogEvent(eventName, parameters);
    }

    public static void Initialize(AnalyticsFacade manager)
    {
        _manager = manager ?? throw new System.ArgumentNullException(nameof(manager));
    }

    public static void LogGameStarted()
    {
        LogEvent("game_started");
    }

    public static void LogMachineUnlocked(string machineId, double cost)
    {
        LogEvent("machine_unlocked", new Dictionary<string, object>
        {
            { "machine_id", machineId },
            { "unlock_cost", cost }
        });
    }

    public static void LogMachineUpgraded(string machineId, int newLevel, double cost)
    {
        LogEvent("machine_upgraded", new Dictionary<string, object>
        {
            { "machine_id", machineId },
            { "level", newLevel },
            { "upgrade_cost", cost }
        });
    }

    public static void LogBoostStarted(float duration, float multiplier)
    {
        LogEvent("boost_started", new Dictionary<string, object>
        {
            { "duration_seconds", duration },
            { "multiplier", multiplier }
        });
    }

    public static void LogBoostFinished(bool finishedOffline = false)
    {
        LogEvent("boost_finished", new Dictionary<string, object> { { "finished_offline", finishedOffline } });
    }

    public static void LogOfflineIncomeApplied(double earnedCoins, float offlineSeconds)
    {
        LogEvent("offline_income_applied", new Dictionary<string, object>
        {
            { "earned_coins", earnedCoins },
            { "offline_seconds", offlineSeconds }
        });
    }

    public static void LogPurchaseSucceeded(string productId, double earnedCoins, string transactionId)
    {
        LogEvent("purchase_succeeded", new Dictionary<string, object>
        {
            { "product_id", productId },
            { "earned_coins", earnedCoins },
            { "transaction_id", transactionId }
        });
    }

    public static void LogPurchaseFailed(string productId, string reason)
    {
        LogEvent("purchase_failed", new Dictionary<string, object>
        {
            { "product_id", productId },
            { "fail_reason", reason }
        });
    }
}
