using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class WolfMovement : BaseMovement
{
    private bool attack = false;
    private bool canAttack = true;
    public int attackCooldown = 2;
    public float attackMoveSpeed = 10f;

    private void Update()
    {
        HandleAttack();
        if (IsAttacking)
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
        IsAttacking = true;
        canAttack = false;
        // do the attack animation
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown);
        // set to true the "canAttack" boolean after a cooldown
        canAttack = true;
    }

}
