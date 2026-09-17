using System;
using UnityEngine;

public sealed class GameLoop : MonoBehaviour
{
    private BoostService _boostService;
    private FactoryModel _factoryModel;

    public void Construct(BoostService boostService, FactoryModel factoryModel)
    {
        _boostService = boostService ?? throw new ArgumentNullException(nameof(boostService));
        _factoryModel = factoryModel ?? throw new ArgumentNullException(nameof(factoryModel));
    }

    private void Update()
    {
        if (_boostService == null || _factoryModel == null)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        _boostService.Tick(deltaTime);
        _factoryModel.Tick(deltaTime);
    }
}
