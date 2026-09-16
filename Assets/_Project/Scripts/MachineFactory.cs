using System;
using UnityEngine;

public class MachineFactory
{

    private readonly MachineView _machinePrefab;
    private readonly Transform _container;
    private readonly WalletModel _wallet;

    public MachineFactory(MachineView machinePrefab, Transform container, WalletModel wallet)
    {
        _machinePrefab = machinePrefab ?? throw new ArgumentNullException(nameof(machinePrefab));
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
    }

    public MachinePresenter CreateMachine(MachineModel model)
    {
        if (model == null) throw new ArgumentNullException(nameof(model));

        MachineView viewInstance = UnityEngine.Object.Instantiate(_machinePrefab, _container);
        return new MachinePresenter(viewInstance, model, _wallet);
    }



    //    private readonly MachineView _machinePrefab;
    //    private readonly Transform _container;

    //    public MachineFactory(MachineView machinePrefab, Transform container)
    //    {
    //        _machinePrefab = machinePrefab != null ? machinePrefab : throw new ArgumentNullException(nameof(machinePrefab));

    //        _container = container != null ? container : throw new ArgumentNullException(nameof(container));
    //    }

    //    public MachinePresenter CreateMachine(MachineConfig config)
    //    {
    //        if (config == null) throw new ArgumentNullException(nameof(config));

    //        // 1. Инстанцируем View из префаба прямо в UI-контейнер (Content у ScrollRect)
    //        MachineView viewInstance = UnityEngine.Object.Instantiate(_machinePrefab, _container);

    //        // 2. Определяем стартовое состояние машины из конфига
    //        EMachineState initialState = config.IsUnlockedByDefault ? EMachineState.Unlocked : EMachineState.Locked;

    //        // 3. Создаем Модель
    //        var model = new MachineModel(config, initialState);

    //        // 4. Связываем View и Model через Presenter и возвращаем его
    //        return new MachinePresenter(viewInstance, model);
    //    }
}
