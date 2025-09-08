using UnityEngine;

public class AttackStateBehaviour : StateMachineBehaviour
{
    // Called when the state finished
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Obtain the WolfMovement script in the same gameobject
        WolfMovement wolf = animator.GetComponent<WolfMovement>();
        if (wolf != null)
        {
            wolf.IsAttacking = false;
        }
    }
}
