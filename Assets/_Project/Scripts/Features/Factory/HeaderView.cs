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
    [SerializeField] private GameObject _boostContainer;
    [SerializeField] private TMP_Text _boostStateText;
    [SerializeField] private Button _boostButton;

    [Header("Store UI")]
    [SerializeField] private GameObject _storeContainer;
    [SerializeField] private TMP_Text _storeRewardText;
    [SerializeField] private TMP_Text _storePriceText;
    [SerializeField] private TMP_Text _storeStatusText;
    [SerializeField] private Button _purchaseButton;

    public event Action OnBoostClicked;
    public event Action OnPurchaseClicked;

    private void Awake()
    {
        if (_boostButton != null)
        {
            _boostButton.onClick.AddListener(HandleBoostClicked);
        }

        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.AddListener(HandlePurchaseClicked);
        }
    }

    private void OnDestroy()
    {
        if (_boostButton != null)
        {
            _boostButton.onClick.RemoveListener(HandleBoostClicked);
        }

        if (_purchaseButton != null)
        {
            _purchaseButton.onClick.RemoveListener(HandlePurchaseClicked);
        }
    }

    public void UpdateEconomy(double balance, double totalProduction)
    {
        if (_balanceText != null)
        {
            _balanceText.text = $"Balance: {FormatNumber(balance)}";
        }

        if (_totalProductionText != null)
        {
            _totalProductionText.text = $"Total Production: +{FormatNumber(totalProduction)}/s";
        }
    }

    public void SetBoostFeatureActive(bool isEnabled)
    {
        if (_boostContainer != null)
        {
            _boostContainer.SetActive(isEnabled);
        }
        else if (_boostButton != null)
        {
            _boostButton.gameObject.SetActive(isEnabled);
        }
    }

    public void SetBoostState(bool isActive)
    {
        if (_boostStateText != null)
        {
            _boostStateText.text = isActive ? "Boost: ACTIVE" : "Boost: READY";
        }

        if (_boostButton != null)
        {
            _boostButton.interactable = !isActive;
        }
    }

    public void SetStoreFeatureActive(bool isEnabled)
    {
        if (_storeContainer != null)
        {
            _storeContainer.SetActive(isEnabled);
        }
    }

    public void SetStoreReward(double reward)
    {
        if (_storeRewardText != null)
        {
            _storeRewardText.text = $"+{FormatNumber(reward)} COINS";
        }
    }

    public void SetStorePrice(string localizedPrice)
    {
        if (_storePriceText != null)
        {
            _storePriceText.text = string.IsNullOrEmpty(localizedPrice) ? "BUY" : $"BUY  {localizedPrice}";
        }
    }

    public void SetStoreStatus(string message)
    {
        if (_storeStatusText != null)
        {
            _storeStatusText.text = message;
        }
    }

    public void SetPurchaseInteractable(bool isInteractable)
    {
        if (_purchaseButton != null)
        {
            _purchaseButton.interactable = isInteractable;
        }
    }

    private void HandleBoostClicked()
    {
        OnBoostClicked?.Invoke();
    }

    private void HandlePurchaseClicked()
    {
        OnPurchaseClicked?.Invoke();
    }

    private static string FormatNumber(double value)
    {
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F2}B";
        if (value >= 1_000_000) return $"{value / 1_000_000:F2}M";
        if (value >= 1_000) return $"{value / 1_000:F1}K";

        return Math.Floor(value).ToString("F0");
    }
}
