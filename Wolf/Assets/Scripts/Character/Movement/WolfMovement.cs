using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class WolfMovement : BaseMovement
{
    private bool attack = false;
    private bool canAttack = true;
    public NetworkVariable<bool> attackDone = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );
    public int attackCooldown = 2;
    public float attackMoveSpeed = 10f;

    private void Update()
    {
        HandleAttack();
        if (IsAttacking.Value)
        {
            // Movement ahead
            Vector3 forward = transform.forward;
            controller.Move(forward * attackMoveSpeed * Time.deltaTime);
        }
    }

    void HandleAttack()
    {
        // if the wolf can attack and the left click is used
        if (parentNetObj.IsOwner && canAttack && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        // set can attack to false for some seconds
        StartAttackServerRpc();
        canAttack = false;
        // do the attack animation
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown);
        // set to true the "canAttack" boolean after a cooldown
        canAttack = true;
        AttackDoneServerRpc(false);
    }


    [ServerRpc(RequireOwnership = false)]
    public void StartAttackServerRpc()
    {
        IsAttacking.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndAttackServerRpc()
    {
        IsAttacking.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AttackDoneServerRpc(bool done)
    {
        attackDone.Value = done;
    }

}
