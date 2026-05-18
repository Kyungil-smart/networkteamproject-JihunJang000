using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Lobbies.Models;

public class OutGameUIView : MonoBehaviour
{
    [Header("아웃게임 판넬들")]
    public GameObject startPanel;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject GuidePanel;   

    [Header("로비 화면 UI")]
    public Button enterLobbyButton;
    public Button controlsButton;      
    public Button quitButton;
    public Button createRoomButton;
    public Button refreshLobbyButton;
    public Button backButton;
    public Transform roomListContainer;
    public GameObject roomButtonPrefab;

    [Header("대기실 화면 UI")]
    public TextMeshProUGUI roomStatusText;
    public Button readyButton;
    public Button startGameButton;

    [Header("가이드 화면 UI")]
    public Button closeControlsButton;
    
    // Presenter가 구독할 이벤트들
    public Action OnEnterLobbyClicked;
    public Action OnControlsClicked;
    public Action OnQuitClicked;
    public Action OnCreateRoomClicked;
    public Action OnRefreshLobbyClicked;
    public Action<Lobby> OnJoinRoomClicked;
    public Action OnBackClicked;
    public Action OnReadyClicked;
    public Action OnStartGameClicked;

    private void Awake()
    {
        if (enterLobbyButton != null) enterLobbyButton.onClick.AddListener(() => OnEnterLobbyClicked?.Invoke());
        if (quitButton != null) quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        createRoomButton.onClick.AddListener(() => OnCreateRoomClicked?.Invoke());
        refreshLobbyButton.onClick.AddListener(() => OnRefreshLobbyClicked?.Invoke());
        readyButton.onClick.AddListener(() => OnReadyClicked?.Invoke());
        startGameButton.onClick.AddListener(() => OnStartGameClicked?.Invoke());
        backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        if (controlsButton != null)
            controlsButton.onClick.AddListener(() => OnControlsClicked?.Invoke());
            
        if (closeControlsButton != null)
            closeControlsButton.onClick.AddListener(() => GuidePanel.SetActive(false));
    }

    public void ShowPanel(GameObject panelToShow)
    {
        startPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
        if (panelToShow != null) panelToShow.SetActive(true); // 해당 패널만 보이게
    }

    public void DrawLobbyList(List<Lobby> lobbies)
    {
        foreach (Transform child in roomListContainer) Destroy(child.gameObject);

        foreach (Lobby lobby in lobbies)
        {
            GameObject btnObj = Instantiate(roomButtonPrefab, roomListContainer);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = $"[{lobby.Name}] ({lobby.Players.Count}/{lobby.MaxPlayers}) Join";
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnJoinRoomClicked?.Invoke(lobby));
        }
    }

    public void UpdateRoomStatusText(string text) => roomStatusText.text = text;

    public void SetRoomButtons(bool isServer, bool canStart)
    {
        readyButton.gameObject.SetActive(!isServer);
        startGameButton.gameObject.SetActive(isServer);
        startGameButton.interactable = canStart;
    }
    
    public void ShowControlsPopup()
    {
        GuidePanel.SetActive(true);
    }
}