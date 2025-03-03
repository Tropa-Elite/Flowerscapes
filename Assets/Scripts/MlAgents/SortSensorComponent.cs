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

        public override ISensor[] CreateSensors()
        {
            Dispose();

            _sensors = new ISensor[] { new SortSensor("Sort Sensor", MainInstaller.Resolve<IGameDataProviderLocator>()) };

            return _sensors;
        }

        public void Dispose()
        {
            _sensors = null;
        }
    }
}