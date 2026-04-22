using Unity.Services.Vivox;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class VoiceChatManager : MonoBehaviour
{
    public static VoiceChatManager Instance;

    private bool isInitialized = false;
    private bool isLoggedIn = false;
    private string currentChannel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task InitializeAndLogin(string playerName)
    {
        Debug.Log(playerName);
        // Initialize Unity Services ONLY once
        if (!isInitialized)
        {
            // Ensure Unity Services are initialized before using any service
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            // Sign in anonymously ONLY if the player is not already signed in
            // Unity may persist authentication between sessions, so this check is required
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            // Initialize Vivox service (voice system)
            await VivoxService.Instance.InitializeAsync();

            isInitialized = true;
        }

        // Login ONLY once
        if (!isLoggedIn)
        {
            // Login to Vivox with the provided player display name
            await VivoxService.Instance.LoginAsync(new LoginOptions()
            {
                DisplayName = playerName
            });

            // Mark as logged in to avoid duplicate logins
            isLoggedIn = true;
        }
    }

    // 🔊 LOBBY VOICE
    public async Task JoinLobbyChannel(string channelName)
    {
        await LeaveCurrentChannel();

        await VivoxService.Instance.JoinGroupChannelAsync(
            channelName,
            ChatCapability.AudioOnly
        );

        currentChannel = channelName;
    }

    // 🎧 PROXIMITY VOICE
    public async Task JoinProximityChannel(string channelName, Channel3DProperties props)
    {
        await LeaveCurrentChannel();

        await VivoxService.Instance.JoinPositionalChannelAsync(
            channelName,
            ChatCapability.AudioOnly,
            props
        );

        currentChannel = channelName;
    }

    public async Task LeaveCurrentChannel()
    {
        if (!string.IsNullOrEmpty(currentChannel))
        {
            await VivoxService.Instance.LeaveChannelAsync(currentChannel);
            currentChannel = null;
        }
    }

    public string GetCurrentChannel() => currentChannel;
}