using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Simulation
{
    [CreateAssetMenu(menuName = Names.GameName + "/Simulation Step Service")]
    public class SimulationStepService : ScriptableObject
    {
        private readonly List<ISimulationDriven> _drivenItems = new List<ISimulationDriven>();
        [CanBeNull] private SimulationDriver _driver;

        public void ExecuteSimulationStep()
        {
            foreach (ISimulationDriven drivenItem in _drivenItems)
            {
                drivenItem.SimulationStepUpdate();
            }
        }

        public void RegisterDriven(ISimulationDriven item)
        {
            _drivenItems.Add(item);
            if (_driver != null)
            {
                item.RegisterDriver(_driver);
            }
        }

        public void DeregisterDriven(ISimulationDriven item)
        {
            if (_driver != null)
            {
                item.DeregisterDriver();
            }
            _drivenItems.Remove(item);
        }

        public void RegisterDriver(SimulationDriver driver)
        {
            Debug.Assert(_driver == null, "SimulationDriver already registered");
            _driver = driver;
            foreach (ISimulationDriven simulationDriven in _drivenItems)
            {
                simulationDriven.RegisterDriver(driver);
            }
        }

        public void DeregisterDriver(SimulationDriver driver)
        {
            Debug.Assert(_driver == driver, "SimulationDriver not registered");
            _driver = null;
            foreach (ISimulationDriven drivenItem in _drivenItems)
            {
                drivenItem.DeregisterDriver();
            }
        }
    }
}