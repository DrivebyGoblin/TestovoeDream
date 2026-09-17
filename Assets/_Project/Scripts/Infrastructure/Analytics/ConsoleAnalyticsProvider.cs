using System.Collections.Generic;
using UnityEngine;

public class ConsoleAnalyticsProvider : IAnalyticsProvider
{
    public void Initialize()
    {
        Debug.Log("[Analytics] Console Provider Initialized.");
    }

    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        string paramStr = string.Empty;
        if (parameters != null)
        {
            var list = new List<string>();
            foreach (var kvp in parameters)
            {
                list.Add($"{kvp.Key}: {kvp.Value}");
            }
            paramStr = " | Params: " + string.Join(", ", list);
        }

        Debug.Log($"<color=cyan>[Analytics Event]</color> {eventName}{paramStr}");
    }
}
