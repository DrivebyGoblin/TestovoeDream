using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineView : MonoBehaviour
{
    [Header("Machine")]
    [SerializeField] private TMP_Text _machineTitleText;
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _productionText;

    [Header("State Panels")]
    [SerializeField] private GameObject _lockedPanel;
    [SerializeField] private GameObject _unlockedPanel;

    [Header("Unlock")]
    [SerializeField] private TMP_Text _unlockCostText;
    [SerializeField] private Button _unlockButton;

    [Header("Upgrade")]
    [SerializeField] private TMP_Text _upgradeCostText;
    [SerializeField] private Button _upgradeButton;

    public event Action OnUnlockClicked;
    public event Action OnUpgradeClicked;

    private void Awake()
    {
        if (_unlockButton != null)
        {
            _unlockButton.onClick.AddListener(() => OnUnlockClicked?.Invoke());
        }

        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.AddListener(() => OnUpgradeClicked?.Invoke());
        }
    }

    private void OnDestroy()
    {
        if (_unlockButton != null) _unlockButton.onClick.RemoveAllListeners();
        if (_upgradeButton != null) _upgradeButton.onClick.RemoveAllListeners();
    }

    public void SetTitle(string title)
    {
        if (_machineTitleText != null) _machineTitleText.text = title;
    }

    public void SetStateText(string stateText)
    {
        if (_stateText != null) _stateText.text = stateText;
    }

    // Переключение ГО-панелей Locked и Unlocked
    public void SetState(bool isUnlocked)
    {
        if (_lockedPanel != null) _lockedPanel.SetActive(!isUnlocked);
        if (_unlockedPanel != null) _unlockedPanel.SetActive(isUnlocked);
    }

    public void UpdateView(int level, double production, double upgradeCost, double unlockCost)
    {
        if (_levelText != null) _levelText.text = $"Lvl {level}";
        if (_productionText != null) _productionText.text = $"+{FormatNumber(production)}/s";
        if (_upgradeCostText != null) _upgradeCostText.text = FormatNumber(upgradeCost);
        if (_unlockCostText != null) _unlockCostText.text = FormatNumber(unlockCost);
    }

    public void SetInteractable(bool canUnlock, bool canUpgrade)
    {
        if (_unlockButton != null) _unlockButton.interactable = canUnlock;
        if (_upgradeButton != null) _upgradeButton.interactable = canUpgrade;
    }

    private string FormatNumber(double value)
    {
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F2}B";
        if (value >= 1_000_000) return $"{value / 1_000_000:F2}M";
        if (value >= 1_000) return $"{value / 1_000:F1}K";

        return Math.Floor(value).ToString("F0");
    }


}
