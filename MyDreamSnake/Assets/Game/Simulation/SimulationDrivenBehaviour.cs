using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Simulation
{
    public abstract class SimulationDrivenBehaviour : MonoBehaviour, ISimulationDriven
    {
        [SerializeField] private SimulationStepService _simulationStepService;
        [CanBeNull] protected SimulationDriver Driver { get; private set; }

#if UNITY_EDITOR
        protected void Reset()
        {
            string[] guids = AssetDatabase.FindAssets($"t: {nameof(SimulationStepService)}");
            Debug.Assert(guids.Length == 1, $"Expected to find exactly one asset of type {nameof(SimulationStepService)}, count is {guids.Length}");
            string guid = guids[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SimulationStepService));
            Debug.Assert(asset != null, $"Asset at path {guid} is null");
            _simulationStepService = (SimulationStepService)asset;
        }
#endif

        protected virtual void OnEnable()
        {
            _simulationStepService.RegisterDriven(this);
        }

        protected virtual void OnDisable()
        {
            _simulationStepService.DeregisterDriven(this);
        }

        public abstract void SimulationStepUpdate();
        
        public virtual void RegisterDriver(SimulationDriver driver)
        {
            Driver = driver;
        }

        public virtual void DeregisterDriver()
        {
            Driver = null;
        }
    }
}