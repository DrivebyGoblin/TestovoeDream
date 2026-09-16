using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderView : MonoBehaviour
{
    [Header("Economy UI")]
    [SerializeField] private TMP_Text _balanceText;
    [SerializeField] private TMP_Text _totalProductionText;

    [Header("Boost UI")]
    [SerializeField] private GameObject _boostContainer; // Контейнер/панель буста для скрытия
    [SerializeField] private TMP_Text _boostStateText;
    [SerializeField] private Button _boostButton;

    public event Action OnBoostClicked;

    private void Awake()
    {
        if (_boostButton != null)
        {
            _boostButton.onClick.AddListener(() => OnBoostClicked?.Invoke());
        }
    }

    private void OnDestroy()
    {
        if (_boostButton != null)
        {
            _boostButton.onClick.RemoveAllListeners();
        }
    }

    public void UpdateEconomy(double balance, double totalProduction)
    {
        if (_balanceText != null)
            _balanceText.text = $"Balance: {FormatNumber(balance)}";

        if (_totalProductionText != null)
            _totalProductionText.text = $"Total Production: +{FormatNumber(totalProduction)}/s";
    }

    // Включает/выключает отображение фичи буста целиком
    public void SetBoostFeatureActive(bool isFeatureEnabled)
    {
        if (_boostContainer != null)
        {
            _boostContainer.SetActive(isFeatureEnabled);
        }
        else if (_boostButton != null)
        {
            _boostButton.gameObject.SetActive(isFeatureEnabled);
        }
    }

    public void SetBoostState(bool isActive)
    {
        if (_boostStateText != null)
            _boostStateText.text = isActive ? "Boost: ACTIVE" : "Boost: READY";

        if (_boostButton != null)
        {
            _boostButton.interactable = !isActive;
        }
    }

    private string FormatNumber(double value)
    {
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F2}B";
        if (value >= 1_000_000) return $"{value / 1_000_000:F2}M";
        if (value >= 1_000) return $"{value / 1_000:F1}K";

        return Math.Floor(value).ToString("F0");
    }
}
