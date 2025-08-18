using Unity.Netcode;
using UnityEngine;

public class DayNightCicle : NetworkBehaviour
{
    // Synced hour from the server
    public NetworkVariable<float> Hour = new NetworkVariable<float>(
        12f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server // only the server can modify the value
    );

    public float startDangerHour = 19f; // Danger starts at 7 PM
    public float endDangerHour = 6f;    // Danger ends at 6 AM

    public Transform Sun; 
    [SerializeField] public float DayDurationInMinutes = 1;

    private float SunX;

    // For client-side interpolation
    private float lastSyncedHour = 1.0f;
    private float lastSyncTime;
    private float networkDelay = 0f; // network latency in seconds
    private bool hasSynced = false;
    
    // this funcion is executed in all clients, when the SERVER changes the HOUR
    void OnHourSynced(float oldValue, float newValue)
    {
        lastSyncedHour = newValue;
        lastSyncTime = Time.time;
        hasSynced = true;

        // Estimate network delay using RTT (half of ping)
        if (IsClient && NetworkManager.Singleton != null)
        {
            // Get the id of the client
            ulong localClientId = NetworkManager.Singleton.LocalClientId;
            // Get transport layer to calculate the RTT
            var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            // Get the Round Trip Time (RTT). Time a message takes to reach the client
            // Divide by 1000f to get the seconds (Rtt is in miliseconds) 
            // and by 1000f again, because we just want half of the trip (client -> server) 
            networkDelay = transport.GetCurrentRtt(localClientId) / 2000f; // RTT en segundos
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // NETCODE
        // Whenever the server updates the hour, call all the connected clientes and execute the funcion OnHourSynced
        Hour.OnValueChanged += OnHourSynced;

        if (!IsServer)
        {
            // Inicializa interpolación con valor actual
            lastSyncedHour = Hour.Value;
            lastSyncTime = Time.time;
            hasSynced = true;
        }
    }

    void OnDisable()
    {
        Hour.OnValueChanged -= OnHourSynced;
    }


    void Update()
    {
        if (!IsServer && !hasSynced)
            return; // still deciding role, don't run anything

        if (IsServer)
        {
            // server updates official hour
            Hour.Value += Time.deltaTime * GetHourSpeed();
            if (Hour.Value >= 24f) Hour.Value = 0f;
        }

        MoveSunClientRpc();
    }

    float GetHourSpeed()
    {
        return 24f / (60f * DayDurationInMinutes);
    }

    // Public getter for current (interpolated) hour
    public float GetCurrentHour()
    {
        if (IsServer)
            return Hour.Value;
        else
        {
            // Calculate how much time has passed since the last hour update was received from the server,
            // and add half of the network round-trip time to compensate for network latency.
            float elapsed = (Time.time - lastSyncTime) + networkDelay;
            // Estimate the current hour on the client by adding the elapsed time (converted to in-game hours)
            // to the last hour value received from the server.
            float simulatedHour = lastSyncedHour + elapsed * GetHourSpeed();

            if (simulatedHour >= 24f)
                simulatedHour -= 24f;

            return simulatedHour;
        }
    }

    
    [ClientRpc] // just executed in the clients
    void MoveSunClientRpc()
    {
        if (Sun != null)
        {
            // all clients and host update visuals
            float currentHour = GetCurrentHour();
            SunRotation(currentHour);
        }
    }


    void SunRotation(float currentHour)
    {
        SunX = AdaptSunGrades(15f * currentHour);
        Sun.localEulerAngles = new Vector3(SunX, 0, 0);

        if (currentHour + 1 < endDangerHour || currentHour - 1 > startDangerHour)
            Sun.GetComponent<Light>().intensity = 0; // Night
        else
            Sun.GetComponent<Light>().intensity = 1; // Day
    }

    // Adapt it to the normal day hours
    float AdaptSunGrades(float degrees)
    {
        return degrees == 0 ? 270f : degrees - 90f;
    }
}