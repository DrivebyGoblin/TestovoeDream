using System;
using UnityEngine;

public class MachineModel
{
    private const int LockedLevel = 0;
    private const int InitialUnlockedLevel = 1;

    public MachineConfig Config { get; }
    public EMachineState State { get; private set; }
    public int Level { get; private set; }

    // Событие: вызывается, когда что-то меняется (купили/апгрейднули)
    public event Action OnUpdated;

    public MachineModel(MachineConfig config, EMachineState initialState)
    {
        Config = config;
        State = initialState;

        if (State == EMachineState.Unlocked)
        {
            Level = InitialUnlockedLevel; // Уровень 1
        }
        else
        {
            Level = LockedLevel; // Уровень 0 (заблокирована)
        }
    }

    

    // 1. Текущий доход в секунду
    public double GetCurrentProduction()
    {
        // Если машина закрыта — доход 0
        if (State == EMachineState.Locked)
        {
            return 0;
        }

        // Вычисляем показатель степени (на 1 уровне степень должна быть 0)
        int power = Level - InitialUnlockedLevel;

        // Формула: БазовыйДоход * (Множитель ^ Степень)
        double multiplierInPower = Math.Pow(Config.ProductionMultiplier, power);
        return Config.BaseProduction * multiplierInPower;
    }

    // 2. Стоимость открытия
    public double GetUnlockCost()
    {
        return Config.BaseUnlockCost;
    }

    // 3. Стоимость следующего улучшение (Lvl -> Lvl + 1)
    public double GetCurrentUpgradeCost()
    {
        if (State == EMachineState.Locked)
        {
            return Config.BaseUpgradeCost;
        }

        int power = Level - InitialUnlockedLevel;
        double multiplierInPower = Math.Pow(Config.CostMultiplier, power);
        return Config.BaseUpgradeCost * multiplierInPower;
    }

   

    // Открыть машину
    public void Unlock()
    {
        if (State == EMachineState.Unlocked) return;

        State = EMachineState.Unlocked;
        Level = InitialUnlockedLevel;

        // Сообщаем презентеру, что данные обновились
        OnUpdated?.Invoke();
    }

    // Поднять уровень
    public void LevelUp()
    {
        if (State == EMachineState.Locked) return;

        Level++;

        // Сообщаем презентеру, что данные обновились
        OnUpdated?.Invoke();
    }

    
}
