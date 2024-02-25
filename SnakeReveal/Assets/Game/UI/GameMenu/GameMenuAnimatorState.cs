using UnityEngine;

namespace Game.UI.GameMenu
{
    public class GameMenuAnimatorState : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var gameMenu = animator.GetComponent<GameMenu>();
            gameMenu.SetAnimatorState(_state);
        }

        [SerializeField] private GameMenu.AnimatorControlledState _state;
    }
}