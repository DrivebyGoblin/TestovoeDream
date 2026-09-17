using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public double Balance;
    public string LastExitTime;
    public float BoostRemainingTime;
    public List<MachineSaveData> Machines = new List<MachineSaveData>();
    public List<string> ProcessedPurchaseIds = new List<string>();
}

public sealed class PurchaseHistory
{
    private readonly HashSet<string> _processedIds = new HashSet<string>();

    public bool IsProcessed(string transactionId)
    {
        return !string.IsNullOrEmpty(transactionId) && _processedIds.Contains(transactionId);
    }

    public void MarkProcessed(string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
        {
            throw new ArgumentException("Transaction ID cannot be empty.", nameof(transactionId));
        }

        _processedIds.Add(transactionId);
    }

    public void Restore(IEnumerable<string> transactionIds)
    {
        _processedIds.Clear();

        if (transactionIds == null)
        {
            return;
        }

        foreach (string transactionId in transactionIds)
        {
            if (!string.IsNullOrEmpty(transactionId))
            {
                _processedIds.Add(transactionId);
            }
        }
    }

    public List<string> CreateSnapshot()
    {
        return new List<string>(_processedIds);
    }
}
