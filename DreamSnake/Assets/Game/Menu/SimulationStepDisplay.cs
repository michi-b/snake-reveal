using System.Globalization;
using Game.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Menu
{
    [RequireComponent(typeof(Text))]
    public class SimulationStepDisplay : SimulationDriverApplier
    {
        private Text _text;

        protected override void OnEnable()
        {
            _text = GetComponent<Text>();
            base.OnEnable();
        }

        protected override void Apply(SimulationDriver driver)
        {
            _text.text = driver.SimulationStepCount.ToString(CultureInfo.InvariantCulture);
        }
    }
}