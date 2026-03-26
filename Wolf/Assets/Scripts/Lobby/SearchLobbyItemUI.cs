using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class SearchLobbyItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text playersText;

    private Lobby lobby;

    private LobbyUI lobbyUIElement;

    public void Setup(string lobbyName, int currentPlayers, int maxPlayers, Lobby lobby, LobbyUI lobbyUI)
    {
        lobbyNameText.text = lobbyName;
        playersText.text = $"{currentPlayers} / {maxPlayers}";
        this.lobby = lobby;

        // Save the lobbyUI
        this.lobbyUIElement = lobbyUI;
    }

    public void JoinLobbyButton()
    {
        lobbyUIElement.JoinLobby(lobby);
    }
}
