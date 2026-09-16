using System;
using UnityEngine;

public class BoostService : MonoBehaviour
{
    private readonly float _multiplier;
    private readonly float _defaultDuration;
    private float _remainingTime;

    // Флаг из конфига: включена ли фича буста вообще
    public bool IsFeatureEnabled { get; }

    public float CurrentMultiplier => IsActive ? _multiplier : 1f;
    public bool IsActive => IsFeatureEnabled && _remainingTime > 0;
    public float RemainingTime => _remainingTime;

    public event Action OnBoostStateChanged;

    public BoostService(bool isFeatureEnabled, float multiplier, float defaultDuration)
    {
        IsFeatureEnabled = isFeatureEnabled;
        _multiplier = multiplier;
        _defaultDuration = defaultDuration;
    }

    public void ActivateBoost()
    {
        if (!IsFeatureEnabled) return;

        _remainingTime = _defaultDuration;
        OnBoostStateChanged?.Invoke();
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        _remainingTime -= deltaTime;

        if (_remainingTime <= 0)
        {
            _remainingTime = 0;
            OnBoostStateChanged?.Invoke();
        }
    }
}
