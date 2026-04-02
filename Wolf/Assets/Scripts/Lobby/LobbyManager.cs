using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Threading.Tasks;
using IngameDebugConsole;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Collections;
using Unity.Services.Vivox;

public class LobbyManager : NetworkBehaviour
{
    public LobbyUI LobbyUI;

    // Player Information
    private string playerName;
    private int MaxNumberOfPlayers = 16;
    private int MinNumberOfPlayers = 10;

    // Lobby Information
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private float lobbyUpdateTimer;
    private float lobbyUpdateTimerMax = 6f;

    public NetworkVariable<FixedString128Bytes> LobbyNameNetwork =
        new NetworkVariable<FixedString128Bytes>(writePerm: NetworkVariableWritePermission.Server);

    private string lobbyPassword;
    public NetworkList<FixedString32Bytes> PlayersList;

    public int GetMinNumberOfPlayers { get { return MinNumberOfPlayers; } }
    public int GetMaxNumberOfPlayers { get { return MaxNumberOfPlayers; } }
    // public string GetLobbyName { get { return lobbyName; } }

    private void Awake()
    {
        PlayersList = new NetworkList<FixedString32Bytes>();
        // Registrar el comando para que puedas escribir "listlobbies" en la consola
        DebugLogConsole.AddCommand("listlobbies", "Lista los lobbies disponibles", ListLobbies);
        // DebugLogConsole.AddCommand("joinlobbybycode", "Join lobby", JoinLobbyByCode);
        // DebugLogConsole.AddCommand<string>("joinrelaywithcode", "Join relay with code", JoinRelayWithCode);
    }
    public override void OnNetworkSpawn()
    {
        PlayersList.OnListChanged += PlayersChanged;

        LobbyNameNetwork.OnValueChanged += OnLobbyNameChanged;
        
        if (!IsServer) 
            OnLobbyNameChanged(default, LobbyNameNetwork.Value);
    }

    private async void Start()
    {
        try
        {
            PlayerPrefs.DeleteAll();
            // initiate the unity services (lobby,...)
            await UnityServices.InitializeAsync();
            
            // listening the authentication event
            AuthenticationService.Instance.SignedIn += () =>
            {
                // Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };
            // sign in anonymously
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error initializing Unity Services: {e.Message}");
        }
    }


    #region LOBBY
    public void CreateLobby()
    {
        // call the async funcion without blocking the main thread
        _ = CreateLobbyAsync();
    }

    private void OnLobbyNameChanged(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        LobbyUI.ShowLobbyPanel(newValue.ToString());
    }
    private string GetUniquePlayerName()
    {
        string baseName = PlayerPrefs.GetString("PlayerName", "Player");
        string sessionSuffix = Random.Range(0, 9).ToString();
        return $"{baseName}_{sessionSuffix}";
    }

    private async Task CreateLobbyAsync()
    {
        try
        {
            playerName = PlayerPrefs.GetString("PlayerName", "Player");

            lobbyPassword = LobbyUI.LobbyPasswordInput.text;

            int maxPlayers = int.Parse(LobbyUI.MaxPlayerSelected.text);

            // Create Lobby
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetLobbyPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "LobbyName", new DataObject(DataObject.VisibilityOptions.Public, LobbyUI.LobbyNameInput.text) } 
                }
            };

            hostLobby = await LobbyService.Instance.CreateLobbyAsync(LobbyNameNetwork.ToString(), maxPlayers, createLobbyOptions);
            joinedLobby = hostLobby;
            
            // Creates a Relay allocation (reserves a Relay server instance for hosting)
            StartHostWithRelay(maxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error creating lobby: {e}");
        }
    }

    // update the lobby, so it still exist after 30 seconds
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby == null)
            return;

        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer <= 0f)
        {
            heartbeatTimer = heartbeatTimerMax;

            // keep lobby awake
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }
    
    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby == null)
            return;

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer <= 0f)
        {
            lobbyUpdateTimer = lobbyUpdateTimerMax;

            // keep lobby awake
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            joinedLobby = lobby;
        }
    }

    public async Task<List<Lobby>> ListLobbies()
    {
        QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
        {
            Count = 25
        };
        // return all the lobbies
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
        return queryResponse.Results;
    }

    #endregion LOBBY

    #region RELAY
    private async void StartHostWithRelay(int MaxConnections)
    {
        try
        {
            // Reserves the relay servidor temporarily
            var allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
            // Get a code to enter the Server
            string JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            // Get the UnityTransport assigned to the Network manager of the scene
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            // configure the UnityTransport to use the Relay allocation just created
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            
            // Update UI
            LobbyNameNetwork.Value = new FixedString128Bytes(LobbyUI.LobbyNameInput.text);
            AddPlayerListServerRpc(playerName);
            
            // Save joinCode in the lobby
            await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, JoinCode) }
                }
            });
            Debug.Log(JoinCode);

            // Inizialize voice chat for host
            await VoiceChatManager.Instance.InitializeAndLogin(playerName);
            await VoiceChatManager.Instance.JoinLobbyChannel(hostLobby.Id);

        } catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelayWithCode(string joinCode, Lobby Lobby)
    {
        if (NetworkManager.Singleton.IsHost) return;
        try
        {
            joinedLobby = Lobby;
            playerName = PlayerPrefs.GetString("PlayerName", "Player");
            // string joinCode = joinedLobby.Data["RelayJoinCode"].Value;
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            // call the "OnClientConnected" function when a client connects
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay join error: {e}");
        }
    }

    #endregion RELAY

    #region CLIENT CONNECTION

    private async void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        string pname = PlayerPrefs.GetString("PlayerName", "Player");
        AddPlayerListServerRpc(pname);

        // Inizialize voice chat for client
        await VoiceChatManager.Instance.InitializeAndLogin(pname);
        await VoiceChatManager.Instance.JoinLobbyChannel(joinedLobby.Id);
    }

    #endregion END CLIENT CONNECTION

    #region PLAYERS

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerListServerRpc(string playerName)
    {
        PlayersList.Add(playerName);
    }
    
    // function called when the players list is updated
    private void PlayersChanged(NetworkListEvent<FixedString32Bytes> change)
    {
        List<string> playersAsStrings = new List<string>();

        foreach (var p in PlayersList)
        {
            playersAsStrings.Add(p.ToString());
        }
        LobbyUI.RefreshLobbyPlayers(playersAsStrings);
    }

    #endregion END PLAYERS

    #region START GAME

    public async Task StartGame()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;
        Channel3DProperties props = new Channel3DProperties(
            25, // audibleDistance
            5,  // conversationalDistance
            1.0f, // fadeIntensity
            AudioFadeModel.InverseByDistance // 👈 obligatorio
        );
        // Change to proximity channel
        await VoiceChatManager.Instance.JoinProximityChannel("GameChannel", props);

        NetworkStarter networkStarter = NetworkManager.Singleton.GetComponent<NetworkStarter>();
        if(networkStarter)
            networkStarter.StartGame();
    }

    #endregion START GAME

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    #region TEMP?

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error creating lobby: {e}");
        }
    }
    
    private async void KickPlayer()
    {
        // remove second player, because first one is the host
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error creating lobby: {e}");
        }
    }

    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetLobbyPlayer()
            };

            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            Debug.Log("Joined lobby with code " + lobbyCode);
            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error creating lobby: {e}");
        }
    }

    // Only for public lobbies
    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error creating lobby: {e}");
        }
    }

    /// PLAYERS /////
    private Player GetLobbyPlayer()
    {
        return new Player // when a player enters, give him a name
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    }
    
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby " + lobby.Name);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " "+ player.Data["PlayerName"].Value);
        }
    }
    #endregion
}