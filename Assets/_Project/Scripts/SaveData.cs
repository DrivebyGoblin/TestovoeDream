using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public double Balance;
    public string LastExitTime; // ISO-8601 string (DateTime.UtcNow)
    public float BoostRemainingTime;
    public List<MachineSaveData> Machines = new List<MachineSaveData>();
}