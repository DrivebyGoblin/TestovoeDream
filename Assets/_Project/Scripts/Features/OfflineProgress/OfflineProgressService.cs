using System;
using UnityEngine;

public class OfflineProgressService
{
    public struct OfflineResult
    {
        public double EarnedCoins;
        public float ElapsedSeconds;
        public float UsedBoostSeconds;
    }

    public OfflineResult Calculate(
        DateTime lastExitTime,
        DateTime currentTime,
        float maxOfflineDuration,
        float remainingBoostTime,
        float boostMultiplier,
        double baseProductionPerSecond)
    {
        OfflineResult result = new OfflineResult();

        double totalElapsedSeconds = (currentTime - lastExitTime).TotalSeconds;
        if (totalElapsedSeconds <= 0) return result;

        // 1. Ограничиваем максимальное время оффлайна из конфига
        float effectiveOfflineTime = (float)Math.Min(totalElapsedSeconds, maxOfflineDuration);
        result.ElapsedSeconds = effectiveOfflineTime;

        if (baseProductionPerSecond <= 0) return result;

        // 2. Вычисляем, сколько времени из оффлайна покрывалось бустом
        float boostTimeUsed = Math.Min(effectiveOfflineTime, Math.Max(0, remainingBoostTime));
        float normalTimeUsed = effectiveOfflineTime - boostTimeUsed;

        result.UsedBoostSeconds = boostTimeUsed;

        // 3. Считаем итоговый доход
        double boostIncome = boostTimeUsed * baseProductionPerSecond * boostMultiplier;
        double normalIncome = normalTimeUsed * baseProductionPerSecond;

        result.EarnedCoins = boostIncome + normalIncome;
        return result;
    }
}
