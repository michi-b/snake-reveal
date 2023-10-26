namespace Game.Simulation
{
    public abstract class SimulationDriverApplier : SimulationDrivenBehaviour
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Driver != null)
            {
                Apply(Driver);
            }
        }

        public override void RegisterDriver(SimulationDriver driver)
        {
            base.RegisterDriver(driver);
            Apply(driver);
        }

        public override void SimulationStepUpdate()
        {
            Apply(Driver);
        }

        protected abstract void Apply(SimulationDriver driver);
    }
}