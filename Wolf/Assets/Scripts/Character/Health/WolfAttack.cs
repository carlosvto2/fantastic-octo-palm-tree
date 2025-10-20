using UnityEngine;
using Unity.Netcode;

public class WolfAttack : NetworkBehaviour
{
    public int damage = 20;
    public WolfMovement wolfMovement;
    
    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        // The wolf must be attacking
        if (wolfMovement != null && wolfMovement.IsAttacking.Value && !wolfMovement.attackDone.Value)
        {
            wolfMovement.AttackDoneServerRpc(true);
            // Obtain the root network object
            NetworkObject otherNetObj = other.GetComponentInParent<NetworkObject>();
            NetworkObject myNetObj = wolfMovement.GetComponentInParent<NetworkObject>();

            // If the network object is the same, ignore
            if (otherNetObj == null || otherNetObj == myNetObj) return;

            // Get the Health component of the other
            Health targetHealth = otherNetObj.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.ApplyDamage(damage);
                wolfMovement.EndAttackServerRpc(); // finish the wolf attack
            }
        }
    }
}
