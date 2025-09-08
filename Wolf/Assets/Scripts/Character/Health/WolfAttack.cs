using UnityEngine;
using Unity.Netcode;

public class WolfAttack : NetworkBehaviour
{
    public int damage = 20;
    public WolfMovement wolfMovement;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

            // The wolf must be attacking
        if (wolfMovement != null && wolfMovement.IsAttacking.Value)
        {
            // Obtener el NetworkObject raíz del otro
            NetworkObject otherNetObj = other.GetComponentInParent<NetworkObject>();
            NetworkObject myNetObj = wolfMovement.GetComponentInParent<NetworkObject>();

            // Si no hay NetworkObject o es el mismo que yo, lo ignoramos
            if (otherNetObj == null || otherNetObj == myNetObj) return;

            // Buscar Health en el otro
            Health targetHealth = otherNetObj.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.ApplyDamage(damage); // este método solo en servidor
            }
        }
    }
}
