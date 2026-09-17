using System;
using UnityEngine;

public sealed class MachineViewFactory
{
    private readonly MachineView _machinePrefab;
    private readonly Transform _container;
    private readonly WalletModel _wallet;

    public MachineViewFactory(MachineView machinePrefab, Transform container, WalletModel wallet)
    {
        _machinePrefab = machinePrefab ?? throw new ArgumentNullException(nameof(machinePrefab));
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
    }

    public MachinePresenter CreateMachine(MachineModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        MachineView viewInstance = UnityEngine.Object.Instantiate(_machinePrefab, _container);
        return new MachinePresenter(viewInstance, model, _wallet);
    }
}