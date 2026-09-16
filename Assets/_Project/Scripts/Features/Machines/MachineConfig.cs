using UnityEngine;
using System;

[Serializable]
public class MachineConfig
{
    [Tooltip("Уникальный идентификатор машины (например, machine_1, machine_2)")]
    public string Id;

    [Tooltip("Отображаемое название машины в интерфейсе")]
    public string Title;

    [Header("Base Economics")]
    [Tooltip("Стоимость первого открытия машины (0 для первой бесплатной машины)")]
    public double BaseUnlockCost;

    [Tooltip("Базовый доход машины в секунду на 1 уровне")]
    public double BaseProduction;

    [Tooltip("Базовая стоимость первого улучшения (Lvl 1 -> Lvl 2)")]
    public double BaseUpgradeCost;

    [Header("Multipliers / Growth")]
    [Tooltip("Коэффициент роста стоимости апгрейда. Формула: BaseCost * (CostMultiplier ^ (Level - 1))")]
    public float CostMultiplier = 1.15f;

    [Tooltip("Коэффициент роста дохода от уровня. Формула: BaseProduction * (ProductionMultiplier ^ (Level - 1))")]
    public float ProductionMultiplier = 1.10f;

    [Tooltip("Если true, машина доступна бесплатно и сразу при старте игры")]
    public bool IsUnlockedByDefault;
}
