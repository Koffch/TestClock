using System;
using UnityEngine;

public class GlobalManager : MonoBehaviour, IDisposable
{
    [SerializeField] private UIController _uiController;
    
    private readonly ITimeModel _timeModel = new TimeModel();
    private readonly ITimeSynchronizer _timeSynchronizer = new TimeSynchronizer();

    private void Awake()
    {
        _uiController.Init(_timeModel, _timeSynchronizer, Synchronize);
        Synchronize();
    }

    private async void Synchronize()
    {
        var time = await _timeSynchronizer.GetTime(_uiController.ServerIndex);
        _timeModel.SetTime(time);
    }

    private void FixedUpdate()
    {
        _timeModel.ShiftTime(Time.fixedDeltaTime);
        if (_timeModel.TimeToCheck)
            Synchronize();
    }

    public void Dispose()
    {
        _uiController.Dispose();
    }
}