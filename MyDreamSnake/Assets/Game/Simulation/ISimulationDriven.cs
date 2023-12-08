namespace Game.Simulation
{
    public interface ISimulationDriven
    {
        void SimulationStepUpdate();
        void RegisterDriver(SimulationDriver driver);
        void DeregisterDriver();
    }
}