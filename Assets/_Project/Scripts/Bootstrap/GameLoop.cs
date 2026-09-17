using System;
using UnityEngine;

public sealed class GameLoop : MonoBehaviour
{
    private BoostService _boostService;
    private FactoryModel _factoryModel;
    private ApplicationSession _session;
    private int _observedResumeCount;

    public void Construct(BoostService boostService, FactoryModel factoryModel, ApplicationSession session)
    {
        _boostService = boostService ?? throw new ArgumentNullException(nameof(boostService));
        _factoryModel = factoryModel ?? throw new ArgumentNullException(nameof(factoryModel));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _observedResumeCount = session.ResumeCount;
    }

    private void Update()
    {
        if (_boostService == null || _factoryModel == null || _session == null || _session.IsSuspended)
        {
            return;
        }

        // The first frame after resuming may include time already paid as offline income.
        if (_observedResumeCount != _session.ResumeCount)
        {
            _observedResumeCount = _session.ResumeCount;
            return;
        }

        float deltaTime = Time.deltaTime;
        _boostService.Tick(deltaTime);
        _factoryModel.Tick(deltaTime);
    }
}
