using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private LocalGameConfig _configSO;

    [Header("UI Dependencies")]
    [SerializeField] private HeaderView _headerView;
    [SerializeField] private MachineView _machinePrefab;
    [SerializeField] private Transform _machinesContainer;

    [Header("Lifecycle & Persistence")]
    [SerializeField] private AppLifecycleScope _lifecycleScope;
    [SerializeField] private GameLoop _gameLoop;

    private WalletModel _wallet;
    private FactoryModel _factoryModel;
    private BoostService _boostService;
    private HeaderPresenter _headerPresenter;
    private StorePresenter _storePresenter;
    private IIAPService _iapService;
    
    private readonly List<MachinePresenter> _machinePresenters = new List<MachinePresenter>();

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_gameLoop == null || _lifecycleScope == null)
        {
            Debug.LogError("[Bootstrap] GameLoop or AppLifecycleScope is missing.");
            return;
        }

        if (_configSO == null)
        {
            Debug.LogError("[Bootstrap] LocalGameConfig is missing.");
            return;
        }

        GameConfig config = _configSO.GetConfig();
        if (config?.Machines == null)
        {
            Debug.LogError("[Bootstrap] Game configuration is invalid.");
            return;
        }
        
        AnalyticsFacade analytics = new AnalyticsFacade();
        analytics.RegisterProvider(new ConsoleAnalyticsProvider());
        AnalyticsEvents.Initialize(analytics);
        AnalyticsEvents.LogGameStarted();

        _wallet = new WalletModel();
        _boostService = new BoostService(config.IsBoostEnabled, config.BoostMultiplier, config.BoostDurationSeconds);

        List<MachineModel> machineModels = CreateMachines(config.Machines);
        _factoryModel = new FactoryModel(machineModels, _wallet, _boostService);

        if (_headerView != null)
        {
            _headerPresenter = new HeaderPresenter(_headerView, _factoryModel, _wallet, _boostService);
        }

        PurchaseHistory purchaseHistory = new PurchaseHistory();
        InitializePersistence(config, machineModels, purchaseHistory);
        _gameLoop.Construct(_boostService, _factoryModel, _lifecycleScope.Session);
        InitializePurchasing(config, purchaseHistory);
    }

    private List<MachineModel> CreateMachines(MachineConfig[] machineConfigs)
    {
        MachineViewFactory viewFactory = new(_machinePrefab, _machinesContainer, _wallet);
        var models = new List<MachineModel>(machineConfigs.Length);

        for (int i = 0; i < machineConfigs.Length; i++)
        {
            MachineConfig machineConfig = machineConfigs[i];
            EMachineState initialState = machineConfig.IsUnlockedByDefault ? EMachineState.Unlocked : EMachineState.Locked;

            MachineModel model = new MachineModel(machineConfig, initialState);
            models.Add(model);
            _machinePresenters.Add(viewFactory.CreateMachine(model));
        }

        return models;
    }

    private void InitializePersistence(GameConfig config, List<MachineModel> machineModels, PurchaseHistory purchaseHistory)
    {
        if (_lifecycleScope == null)
        {
            Debug.LogError("[Bootstrap] AppLifecycleScope is missing.");
            return;
        }

        _lifecycleScope.Construct(_wallet, _factoryModel, _boostService, machineModels, new SaveService(), new OfflineProgressService(),purchaseHistory,config);

        _lifecycleScope.RestoreStateAndProcessOffline();
    }

    private void InitializePurchasing(GameConfig config, PurchaseHistory purchaseHistory)
    {
        if (_headerView == null)
        {
            return;
        }

        if (!config.IsPurchasingEnabled)
        {
            _headerView.SetStoreFeatureActive(false);
            return;
        }

        if (_lifecycleScope == null)
        {
            _headerView.SetStoreFeatureActive(true);
            _headerView.SetStoreStatus("Save system unavailable");
            _headerView.SetPurchaseInteractable(false);
            return;
        }

        try
        {
            _iapService = new UnityIAPService();
            _storePresenter = new StorePresenter(_headerView, _wallet, _iapService, purchaseHistory, _lifecycleScope.SaveGameState, config.CoinsPackProductId, config.CoinsPackReward);

        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            _headerView.SetStoreFeatureActive(true);
            _headerView.SetStoreStatus("IAP configuration is invalid");
            _headerView.SetPurchaseInteractable(false);
        }
    }

    private async void Start()
    {
        if (_storePresenter == null)
        {
            return;
        }

        try
        {
            await _storePresenter.InitializeAsync();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
            if (this == null || _headerView == null)
            {
                return;
            }

            _headerView.SetStoreStatus("IAP initialization failed");
            _headerView.SetPurchaseInteractable(false);
        }
    }

    private void OnDestroy()
    {
        _storePresenter?.Dispose();
        _iapService?.Dispose();
        _headerPresenter?.Dispose();

        for (int i = 0; i < _machinePresenters.Count; i++)
        {
            _machinePresenters[i]?.Dispose();
        }

        _machinePresenters.Clear();
    }
}
