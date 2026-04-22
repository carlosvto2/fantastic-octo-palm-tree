using UnityEngine;
using Unity.Netcode;
using Unity.Services.Vivox;

public class ProximityVoiceController : NetworkBehaviour
{
    [SerializeField] private Transform localPlayerHead;

    private float _nextUpdateTime = 0f;
    private const float updateInterval = 0.1f;

    void Update()
    {
        if (!IsLocalPlayer)
            return;

        // If no active channel, do nothing
        string currentChannel = VoiceChatManager.Instance.GetCurrentChannel();
        if (string.IsNullOrEmpty(currentChannel))
            return;

        if (Time.time < _nextUpdateTime)
            return;

        _nextUpdateTime = Time.time + updateInterval;

        Update3DPosition(currentChannel);
    }

    private void Update3DPosition(string channelName)
    {
        if (localPlayerHead == null)
            return;

        VivoxService.Instance.Set3DPosition(
            localPlayerHead.gameObject,
            channelName
        );
    }

    public void SetPlayerHead(Transform head)
    {
        localPlayerHead = head;
    }
}