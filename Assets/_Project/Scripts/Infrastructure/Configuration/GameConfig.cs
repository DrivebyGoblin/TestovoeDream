using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [Header("Machines")]
    public MachineConfig[] Machines;

    [Header("Boost")]
    public bool IsBoostEnabled = true;
    [Min(1f)] public float BoostMultiplier = 2f;
    [Min(0f)] public float BoostDurationSeconds = 30f;

    [Header("Offline Production")]
    [Min(0f)] public float MaxOfflineSeconds = 86400f;

    [Header("In-App Purchases")]
    public bool IsPurchasingEnabled = true;
    public string CoinsPackProductId = "coins_pack_small";
    [Min(1f)] public double CoinsPackReward = 1000d;
}
