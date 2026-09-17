using System.Collections.Generic;
using UnityEngine;

public class AnalyticsFacade
{
    private readonly List<IAnalyticsProvider> _providers = new List<IAnalyticsProvider>();

    public void RegisterProvider(IAnalyticsProvider provider)
    {
        if (provider != null && !_providers.Contains(provider))
        {
            try
            {
                provider.Initialize();
                _providers.Add(provider);
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[Analytics] {provider.GetType().Name} initialization failed: {exception.Message}");
            }
        }
    }

    // Универсальный метод отправки события всем провайдерам
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        for (int i = 0; i < _providers.Count; i++)
        {
            try
            {
                _providers[i].LogEvent(eventName,
                    parameters == null ? null : new Dictionary<string, object>(parameters));
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[Analytics] {_providers[i].GetType().Name} failed to send {eventName}: {exception.Message}");
            }
        }
    }
}
