using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyUI : MonoBehaviour
{
    // Buttons
    public Button StartButton;
    public Button CreateLobbyButton;
    public Button StartGameButton;
    public Button SearchLobbiesButt;

    // TextMeshPro
    public TextMeshProUGUI PlayerNameInput;
    public TextMeshProUGUI LobbyNameInput;
    public TextMeshProUGUI LobbyNameShow;
    public TextMeshProUGUI LobbyPasswordInput;
    public TextMeshProUGUI MaxPlayerSelected;
    public TextMeshProUGUI PlayersJoinedLobby;

    // Variables
    private int MinNumberOfPlayers;
    private int MaxNumberOfPlayers;
    private int CurrentMaxPlayers;
    public LobbyManager LobbyManager;
    private bool IsSearchingLobbies = false;

    // Objects

    public Transform PlayersListPanel;
    public GameObject PlayerLobbyItem;
    public GameObject PlayerInfoPanel;
    public GameObject LobbyPanelCreation;
    public GameObject LobbyPlayersPanel;
    public Transform SearchLobbyPanel;
    public GameObject SearchLobbyItem;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // PlayerPrefs.DeleteAll();
        if(!LobbyManager) return;
        MinNumberOfPlayers = LobbyManager.GetMinNumberOfPlayers;
        MaxNumberOfPlayers = LobbyManager.GetMaxNumberOfPlayers;
        CheckLobbySettings();
    }

    private void CheckLobbySettings()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            ShowLobbyPanelCreation();
        }
    }

    #region PLAYER CREATION
    public void SavePlayerInfo()
    {
        PlayerPrefs.SetString("PlayerName", PlayerNameInput.text);
        PlayerPrefs.Save();
        ShowLobbyPanelCreation();
    }

    private void ShowLobbyPanelCreation()
    {
        // Hide/show the panels
        PlayerInfoPanel.SetActive(false);
        LobbyPanelCreation.SetActive(true);
        // Hide/show buttons
        StartButton.gameObject.SetActive(false);
        CreateLobbyButton.gameObject.SetActive(true);
    }

    #endregion END PLAYER CREATION

    #region LOBBY CREATION
    public void ShowLobbyPanel(string LobbyName)
    {
        // Hide/show the panels
        LobbyPanelCreation.SetActive(false);
        LobbyPlayersPanel.SetActive(true);
        // Hide/show buttons
        CreateLobbyButton.gameObject.SetActive(false);
        StartGameButton.gameObject.SetActive(true);

        // Show the name of the lobby
        LobbyNameShow.text = LobbyName;

        FillLobbyWithPlayerSlots();
    }

    public void RightArrowPressed()
    {
        // Increment the max number of players
        CurrentMaxPlayers = int.Parse(MaxPlayerSelected.text);
        if(CurrentMaxPlayers < MaxNumberOfPlayers)
            CurrentMaxPlayers++;
        MaxPlayerSelected.text = CurrentMaxPlayers.ToString();
    }

    public void LeftArrowPressed()
    {
        // Decrease the max number of players
        int CurrentMaxPlayers = int.Parse(MaxPlayerSelected.text);
        if (CurrentMaxPlayers > MinNumberOfPlayers)
            CurrentMaxPlayers--;
        MaxPlayerSelected.text = CurrentMaxPlayers.ToString();
    }
    
    public void CreateLobby()
    {
        if(!LobbyManager) return;
        
        LobbyManager.CreateLobby();
        CreateLobbyButton.interactable = false;
    }

    #endregion

    #region LOBBY SHOW PLAYERS

    public void RefreshLobbyPlayers(List<string> playerNamesList)
    {
        if (!PlayersListPanel) return;

        int totalSlots = PlayersListPanel.childCount;

        for (int i = 0; i < totalSlots; i++)
        {
            Transform slot = PlayersListPanel.GetChild(i);
            Image slotImage = slot.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI slotName = slot.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

            if (i < playerNamesList.Count)
            {
                // Mostrar jugador
                slotImage.enabled = true;
                slotName.text = playerNamesList[i];
            }
            else
            {
                // Vaciar slot
                slotImage.enabled = false;
                slotName.text = "Player";
            }
        }

        // Update number of players in UI
        PlayersJoinedLobby.text = playerNamesList.Count.ToString();
    }
    
    private void FillLobbyWithPlayerSlots()
    {
        CurrentMaxPlayers = int.Parse(MaxPlayerSelected.text);
        for (int i = 0; i < CurrentMaxPlayers; i++)
        {
            Instantiate(PlayerLobbyItem, PlayersListPanel);
        }
    }

    #endregion END LOBBY SHOW PLAYERS

    #region SEARCH LOBBY

    public void SearchLobbiesButton()
    {
        if (IsSearchingLobbies) return;
        IsSearchingLobbies = true;

        // Deactivate search lobby button temporarily
        SearchLobbiesButt.interactable = false;
        _ = SearchLobbiesAsync();
    }

    private async Task SearchLobbiesAsync()
    {
        try
        {
            if (!LobbyManager) return;
            ClearSearchLobbyList();

            List<Lobby> availableLobbies = await LobbyManager.ListLobbies();

            foreach (Lobby lobbyInfo in availableLobbies)
            {
                AddLobbyInfo(lobbyInfo);
            }
        }
        finally
        {
            // Not searching anymore and make the search lobby button interactable again
            IsSearchingLobbies = false;
            SearchLobbiesButt.interactable = true;
        }
    }

    private void AddLobbyInfo(Lobby LobbyInfo)
    {
        GameObject SearchLobbyIt = Instantiate(SearchLobbyItem, SearchLobbyPanel);

        SearchLobbyItemUI LobbyItemScript = SearchLobbyIt.GetComponent<SearchLobbyItemUI>();
        string lobbyName = LobbyInfo.Data.ContainsKey("LobbyName") ? 
                    LobbyInfo.Data["LobbyName"].Value : "Unnamed Lobby";

        // Initialize the lobby available
        LobbyItemScript.Setup(
            lobbyName,
            LobbyInfo.Players.Count,
            LobbyInfo.MaxPlayers,
            LobbyInfo,
            this
        );
    }

    private void ClearSearchLobbyList()
    {
        foreach (Transform child in SearchLobbyPanel)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion SEARCH LOBBY

    #region JOIN LOBBY

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            // Get the lobby to join using the ID
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

            // Obtain the Relay Code to used saved in the lobby to join
            if (!joinedLobby.Data.TryGetValue("RelayCode", out DataObject relayData))
            {
                Debug.LogError("RelayCode not found in lobby");
                return;
            }

            string joinCode = relayData.Value;

            // Tell the LobbyManager to connect to the corresponding lobby using the Join Code
            if (!LobbyManager) return;
            LobbyManager.JoinRelayWithCode(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Join lobby error: {e}");
        }
    }

    #endregion JOIN LOBBY

    #region START
    public void StartGameButtonPressed()
    {
        if(!LobbyManager) return;
        
        LobbyManager.StartGame();
    }

    #endregion START
}
