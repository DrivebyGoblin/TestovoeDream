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

    private WalletModel _wallet;
    private FactoryModel _factoryModel;
    private BoostService _boostService;
    private HeaderPresenter _headerPresenter;
    private readonly List<MachinePresenter> _machinePresenters = new List<MachinePresenter>();

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_configSO == null) return;

        GameConfig config = _configSO.GetConfig();
        if (config == null || config.Machines == null) return;

        _wallet = new WalletModel(initialBalance: 0);

        // 1. Создаем BoostService с параметрами из конфига
        _boostService = new BoostService(config.IsBoostEnabled, config.BoostMultiplier, config.BoostDurationSeconds);

        // 2. Фабрика и карточки
        MachineFactory machineFactory = new MachineFactory(_machinePrefab, _machinesContainer, _wallet);
        List<MachineModel> machineModels = new List<MachineModel>();

        for (int i = 0; i < config.Machines.Length; i++)
        {
            MachineConfig machineConfig = config.Machines[i];
            EMachineState initialState = machineConfig.IsUnlockedByDefault
                ? EMachineState.Unlocked
                : EMachineState.Locked;

            MachineModel machineModel = new MachineModel(machineConfig, initialState);
            machineModels.Add(machineModel);

            MachinePresenter presenter = machineFactory.CreateMachine(machineModel);
            _machinePresenters.Add(presenter);
        }

        // 3. Создаем FactoryModel
        _factoryModel = new FactoryModel(machineModels, _wallet, _boostService);

        // 4. Презентер UI
        if (_headerView != null)
        {
            _headerPresenter = new HeaderPresenter(_headerView, _factoryModel, _wallet, _boostService);
        }

        // 5. Инициализируем переданный через Инспектор AppLifecycleScope
        if (_lifecycleScope != null)
        {
            SaveService saveService = new SaveService();
            OfflineProgressService offlineService = new OfflineProgressService();

            _lifecycleScope.Construct(
                _wallet,
                _factoryModel,
                _boostService,
                machineModels,
                saveService,
                offlineService,
                config
            );

            _lifecycleScope.RestoreStateAndProcessOffline();
        }
        else
        {
            Debug.LogWarning("[Bootstrap] AppLifecycleScope is missing in Inspector!");
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // Каждый кадр уменьшаем таймер буста и генерируем монеты
        _boostService?.Tick(deltaTime);
        _factoryModel?.Tick(deltaTime);
    }

    private void OnDestroy()
    {
        _headerPresenter?.Dispose();

        for (int i = 0; i < _machinePresenters.Count; i++)
        {
            _machinePresenters[i]?.Dispose();
        }

        _machinePresenters.Clear();
    }



}
