using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

public class OutGameUIPresenter : MonoBehaviour
{
    public static OutGameUIPresenter Instance { get; private set; }
    [SerializeField] private OutGameUIView _view;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        
        _view.OnEnterLobbyClicked += GoToLobby;
        _view.OnQuitClicked += () => {
            Debug.Log("게임 종료!");
            Application.Quit(); // 게임 끄기
        };
        
        _view.OnCreateRoomClicked += () => _ = CloudLobbyManager.Instance.CreateLobby("Room");
        _view.OnRefreshLobbyClicked += HandleRefreshLobby;
        _view.OnJoinRoomClicked += (lobby) => _ = CloudLobbyManager.Instance.JoinLobby(lobby);
        _view.OnBackClicked += () => _view.ShowPanel(_view.startPanel);
        _view.OnReadyClicked += () => CloudRoomManager.Instance.ToggleReady();
        _view.OnStartGameClicked += () => CloudRoomManager.Instance.StartGame();
        _view.OnControlsClicked += () => _view.ShowControlsPopup();
        
        _view.ShowPanel(_view.startPanel);
    }

    private void Update()
    {
        if (_view.startPanel.activeSelf) return;

        // 방에 접속하지 않았다면 로비 화면
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (!_view.lobbyPanel.activeSelf) _view.ShowPanel(_view.lobbyPanel);
        }
        // 방에 접속했다면 대기실 화면
        else
        {
            if (!_view.roomPanel.activeSelf) _view.ShowPanel(_view.roomPanel);
            RefreshRoomView(); // 대기실 유저 목록 갱신
        }
    }
    
    public void GoToLobby() 
    {
        _view.ShowPanel(_view.lobbyPanel);
        HandleRefreshLobby();
    }
    
    public void OnLoginSuccess() // CloudAuthManager에서 로그인 완료 시 호출
    {
        _view.ShowPanel(_view.lobbyPanel);
        HandleRefreshLobby();
    }

    private async void HandleRefreshLobby()
    {
        List<Lobby> lobbies = await CloudLobbyManager.Instance.GetLobbyList();
        _view.DrawLobbyList(lobbies);
    }

    private void RefreshRoomView()
    {
        if (CloudRoomManager.Instance == null || !CloudRoomManager.Instance.IsSpawned) return;

        int playerCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
        
        string statusText = $"Players: {playerCount}\n\n";

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            bool isHost = (clientId == NetworkManager.ServerClientId);
            bool isReady = CloudRoomManager.Instance.readyClients.Contains(clientId);
            
            string myTag = (clientId == NetworkManager.Singleton.LocalClientId) ? " (Me)" : "";
            statusText += $"Player {clientId}{myTag} : {(isHost ? "[ Host ]" : (isReady ? "[ Ready ]" : "Waiting..."))}\n";
        }

        _view.UpdateRoomStatusText(statusText);
        _view.SetRoomButtons(NetworkManager.Singleton.IsServer, CloudRoomManager.Instance.IsAllReady());
    }
}