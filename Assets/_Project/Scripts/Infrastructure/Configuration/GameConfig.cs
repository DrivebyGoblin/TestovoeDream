using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [Header("Machines Configuration")]
    public MachineConfig[] Machines;


    [Tooltip("Возможность включить/выключить Boost")]
    [Header("Boost Settings")]
    public bool IsBoostEnabled = true;

    [Tooltip("Множитель производства")]
    public float BoostMultiplier = 2.0f;

    [Tooltip("Длительность буста в секундах")]
    public float BoostDurationSeconds = 30f;


    [Header("Offline Production")]
    [Tooltip("Максимальное время offline (например, 24 часа = 86400 сек)")]
    public float MaxOfflineSeconds = 86400f;
}
