using System;
using Game.Logic;
using GameLovers.Services;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Game.MlAgents
{
    [AddComponentMenu("ML Agents/Sort Sensor", 50)]
    public class SortSensorComponent : SensorComponent, IDisposable
    {
	  private ISensor[] _sensors;
	  private IGameDataProviderLocator _gameDataProvider;

	  private void Awake()
	  {
		_gameDataProvider = MainInstaller.Resolve<IGameDataProviderLocator>();
	  }

	  public override ISensor[] CreateSensors()
	  {
		Dispose();
		
		_sensors = new ISensor[] { new SortSensor("Sort Sensor", _gameDataProvider) };
		
		return _sensors;
	  }

	  public void Dispose()
	  {
		_sensors = null;
	  }
    }
}