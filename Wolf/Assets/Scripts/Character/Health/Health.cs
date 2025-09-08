using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;

    // NetworkVariable to synchronize the health
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    [Header("UI Prefabs")]
    public HealthBar3D worldHealthBarPrefab;
    
    public HealthBar PlayerHealthBar;
    private HealthBar3D worldHealthBarInstance;
    public Transform HealthBarPosition;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            currentHealth.Value = maxHealth;

        NetworkObject rootNetObj = GetComponentInParent<NetworkObject>();
        if (rootNetObj.IsOwner)
            SpawnPlayerHealth();
        else
            SpawnOtherPlayersHealth();
        // When the health changes, update the UI in all clients
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnDestroy()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void SpawnPlayerHealth()
    {
        if (!PlayerHealthBar) return;
        PlayerHealthBar.ShowHealth();
        PlayerHealthBar.SetMaxHealth(currentHealth.Value);
    }

    /// <summary>
    /// Instantiates a world-space health bar for non-local players or NPCs.
    /// The bar follows the character and shows their current health.
    /// </summary>
    private void SpawnOtherPlayersHealth()
    {
        if (worldHealthBarPrefab)
        {
            worldHealthBarInstance = Instantiate(worldHealthBarPrefab, HealthBarPosition.position, Quaternion.identity);
            worldHealthBarInstance.target = HealthBarPosition;
            worldHealthBarInstance.SetMaxHealth(currentHealth.Value);
        }
    }

    void OnHealthChanged(int oldValue, int newValue)
    {
        // Adjust his onw players health bar
        if (PlayerHealthBar)
            PlayerHealthBar.SetHealth(newValue);
        // Adjust his own players health bar IN OTHER CLIENTS
        if (worldHealthBarInstance)
            worldHealthBarInstance.SetHealth(newValue);
        if (newValue <= 0)
        {
            // Death animation
        }
    }

    public void ApplyDamage(int damage)
    {
        if (currentHealth.Value <= 0) return;
        currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);
    }

    // Heal
    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(int amount)
    {
        if (currentHealth.Value <= 0) return;
        currentHealth.Value = Mathf.Min(currentHealth.Value + amount, maxHealth);
    }
}

