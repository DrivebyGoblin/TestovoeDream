using System;


public class BoostService
{
    private readonly float _multiplier;
    private readonly float _defaultDuration;
    private readonly float _defaultValue = 1f;
    private float _remainingTime;

    // Флаг из конфига: включена ли фича буста вообще
    public bool IsFeatureEnabled { get; }

    public float CurrentMultiplier => IsActive ? _multiplier : _defaultValue;
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
        if (!IsFeatureEnabled || _defaultDuration <= 0) return;

        _remainingTime = _defaultDuration;
        AnalyticsEvents.LogBoostStarted(_defaultDuration, _multiplier);
        OnBoostStateChanged?.Invoke();
    }

    public void Tick(float deltaTime)
    {
        if (!IsActive) return;

        _remainingTime -= deltaTime;

        if (_remainingTime <= 0)
        {
            _remainingTime = 0;
            AnalyticsEvents.LogBoostFinished();
            OnBoostStateChanged?.Invoke();
        }
    }

    // Метод для явной установки времени (используется при загрузке оффлайн-прогресса)
    public void SetRemainingTime(float time)
    {
        _remainingTime = Math.Max(0f, time);
        OnBoostStateChanged?.Invoke();
    }
}
