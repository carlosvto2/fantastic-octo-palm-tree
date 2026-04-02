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
        // Initialize Unity Services ONLY once
        if (!isInitialized)
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await VivoxService.Instance.InitializeAsync();

            isInitialized = true;
        }

        // Login ONLY once
        if (!isLoggedIn)
        {
            await VivoxService.Instance.LoginAsync(new LoginOptions()
            {
                DisplayName = playerName
            });

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
        Debug.Log("Joined lobby voice");
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
        Debug.Log("Joined proximity voice");
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