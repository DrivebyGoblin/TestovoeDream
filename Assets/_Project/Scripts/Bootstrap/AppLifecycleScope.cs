using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class AppLifecycleScope : MonoBehaviour
{
    private WalletModel _wallet;
    private FactoryModel _factoryModel;
    private BoostService _boostService;
    private List<MachineModel> _machines;
    private SaveService _saveService;
    private OfflineProgressService _offlineService;
    private PurchaseHistory _purchaseHistory;
    private GameConfig _config;

    public void Construct(
        WalletModel wallet,
        FactoryModel factoryModel,
        BoostService boostService,
        List<MachineModel> machines,
        SaveService saveService,
        OfflineProgressService offlineService,
        PurchaseHistory purchaseHistory,
        GameConfig config)
    {
        _wallet = wallet;
        _factoryModel = factoryModel;
        _boostService = boostService;
        _machines = machines;
        _saveService = saveService;
        _offlineService = offlineService;
        _purchaseHistory = purchaseHistory;
        _config = config;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGameState();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameState();
    }

    public void SaveGameState()
    {
        if (_wallet == null)
        {
            return;
        }

        SaveData data = new SaveData
        {
            Balance = _wallet.Balance,
            LastExitTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            BoostRemainingTime = _boostService.RemainingTime,
            Machines = new List<MachineSaveData>(),
            ProcessedPurchaseIds = _purchaseHistory.CreateSnapshot()
        };

        for (int i = 0; i < _machines.Count; i++)
        {
            data.Machines.Add(new MachineSaveData
            {
                Id = _machines[i].Config.Id,
                Level = _machines[i].Level,
                IsUnlocked = _machines[i].State == EMachineState.Unlocked
            });
        }

        _saveService.Save(data);
        Debug.Log($"[Save] Progress saved at {data.LastExitTime}.");
    }

    public void RestoreStateAndProcessOffline()
    {
        SaveData data = _saveService.Load();
        if (data == null)
        {
            Debug.Log("[Save] No previous progress found.");
            return;
        }

        _purchaseHistory.Restore(data.ProcessedPurchaseIds);
        RestoreMachines(data.Machines);
        _wallet.Add(data.Balance);
        ApplyOfflineProgress(data);
    }

    private void RestoreMachines(List<MachineSaveData> savedMachines)
    {
        if (savedMachines == null)
        {
            return;
        }

        for (int i = 0; i < savedMachines.Count; i++)
        {
            MachineSaveData savedMachine = savedMachines[i];
            MachineModel model = _machines.Find(machine => machine.Config.Id == savedMachine.Id);
            if (model == null)
            {
                continue;
            }

            if (savedMachine.IsUnlocked)
            {
                model.Unlock();
            }

            for (int level = 1; level < savedMachine.Level; level++)
            {
                model.LevelUp();
            }
        }
    }

    private void ApplyOfflineProgress(SaveData data)
    {
        if (!DateTime.TryParse(
                data.LastExitTime,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out DateTime lastExit))
        {
            Debug.LogWarning($"[Offline] Invalid saved timestamp: {data.LastExitTime}");
            return;
        }

        DateTime currentTime = DateTime.UtcNow;
        double elapsedSeconds = Math.Max(0, (currentTime - lastExit.ToUniversalTime()).TotalSeconds);
        double baseProduction = _factoryModel.GetBaseProductionWithoutBoost();
        OfflineProgressService.OfflineResult result = _offlineService.Calculate(
            lastExitTime: lastExit,
            currentTime: currentTime,
            maxOfflineDuration: _config.MaxOfflineSeconds,
            remainingBoostTime: _boostService.IsFeatureEnabled ? data.BoostRemainingTime : 0f,
            boostMultiplier: _config.BoostMultiplier,
            baseProductionPerSecond: baseProduction);

        _wallet.Add(result.EarnedCoins);

        if (result.EarnedCoins > 0)
        {
            AnalyticsEvents.LogOfflineIncomeApplied(result.EarnedCoins, result.ElapsedSeconds);
        }

        // Boost uses real elapsed time, independently of the offline income cap.
        float remainingBoostTime = (float)Math.Max(0, data.BoostRemainingTime - elapsedSeconds);
        _boostService.SetRemainingTime(remainingBoostTime);
        if (_boostService.IsFeatureEnabled && data.BoostRemainingTime > 0 && remainingBoostTime <= 0)
        {
            AnalyticsEvents.LogBoostFinished(finishedOffline: true);
        }

        // Persist the applied income and expired boost so the next launch cannot replay them.
        SaveGameState();

        Debug.Log(
            $"[Offline] Elapsed: {result.ElapsedSeconds:F1}s, " +
            $"production: {baseProduction:F1}/s, earned: {result.EarnedCoins:F0}.");
    }
}
