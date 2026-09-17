using System.Collections.Generic;
using UnityEngine;

public interface IAnalyticsProvider
{
    void Initialize();
    void LogEvent(string eventName, Dictionary<string, object> parameters = null);
}