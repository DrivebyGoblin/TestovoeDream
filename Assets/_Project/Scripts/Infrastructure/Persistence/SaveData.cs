using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public double Balance;
    public string LastExitTime;
    public float BoostRemainingTime;
    public List<MachineSaveData> Machines = new List<MachineSaveData>();
}