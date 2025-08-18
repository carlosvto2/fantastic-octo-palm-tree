using UnityEngine;
using Unity.Netcode.Components;

public class ClientNetworkAnimator : NetworkAnimator
{
    // Override to indicate that this object uses client authority instead of server authority.
    // This allows clients to control their own Animator state changes,
    // which is necessary when using NetworkAnimator on a client-driven character.
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
