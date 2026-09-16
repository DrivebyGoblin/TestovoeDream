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
    private GameConfig _config;

    public void Construct(
        WalletModel wallet,
        FactoryModel factoryModel,
        BoostService boostService,
        List<MachineModel> machines,
        SaveService saveService,
        OfflineProgressService offlineService,
        GameConfig config)
    {
        _wallet = wallet;
        _factoryModel = factoryModel;
        _boostService = boostService;
        _machines = machines;
        _saveService = saveService;
        _offlineService = offlineService;
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
        if (_wallet == null) return;

        SaveData data = new SaveData
        {
            Balance = _wallet.Balance,
            LastExitTime = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            BoostRemainingTime = _boostService.RemainingTime,
            Machines = new List<MachineSaveData>()
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
        Debug.Log($"<color=cyan>[SAVE]</color> Сохранено! Время: {data.LastExitTime}");
    }

    public void RestoreStateAndProcessOffline()
    {
        SaveData data = _saveService.Load();
        if (data == null)
        {
            Debug.Log("<color=orange>[OFFLINE]</color> Сохранение не найдено (первый запуск).");
            return;
        }

        // 1. Восстанавливаем машины
        for (int i = 0; i < data.Machines.Count; i++)
        {
            var savedMachine = data.Machines[i];
            var model = _machines.Find(m => m.Config.Id == savedMachine.Id);
            if (model != null)
            {
                if (savedMachine.IsUnlocked) model.Unlock();
                for (int lvl = 1; lvl < savedMachine.Level; lvl++) model.LevelUp();
            }
        }

        // 2. Восстанавливаем сохранившийся баланс
        _wallet.Add(data.Balance);

        // 3. Парсим дату с InvariantCulture для защиты от локали
        if (DateTime.TryParse(data.LastExitTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime lastExit))
        {
            double baseProduction = _factoryModel.GetBaseProductionWithoutBoost();

            var result = _offlineService.Calculate(
                lastExitTime: lastExit,
                currentTime: DateTime.UtcNow,
                maxOfflineDuration: _config.MaxOfflineSeconds,
                remainingBoostTime: data.BoostRemainingTime,
                boostMultiplier: _config.BoostMultiplier,
                baseProductionPerSecond: baseProduction
            );

            Debug.LogWarning($"<color=yellow>[OFFLINE LOG]</color> Прошло сек: {result.ElapsedSeconds:F1}s | " +
                             $"Базовый доход/сек: {baseProduction} | " +
                             $"Начислено оффлайн-монет: {result.EarnedCoins:F0}");

            _wallet.Add(result.EarnedCoins);

            float newBoostTime = Mathf.Max(0, data.BoostRemainingTime - result.ElapsedSeconds);
            _boostService.SetRemainingTime(newBoostTime);
        }
        else
        {
            Debug.LogError($"[OFFLINE ERROR] Не удалось распарсить дату: {data.LastExitTime}");
        }
    }
}
