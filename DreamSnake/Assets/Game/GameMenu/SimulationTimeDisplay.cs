using System.Globalization;
using Game.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameMenu
{
    [RequireComponent(typeof(Text))]
    public class SimulationTimeDisplay : SimulationDriverApplier
    {
        private Text _text;

        protected override void OnEnable()
        {
            _text = GetComponent<Text>();
            base.OnEnable();
        }

        protected override void Apply(SimulationDriver driver)
        {
            _text.text = driver.SimulationTime.ToString("F2", CultureInfo.InvariantCulture);
        }
    }
}