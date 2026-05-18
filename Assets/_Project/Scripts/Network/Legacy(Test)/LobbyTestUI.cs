using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyTestUI : MonoBehaviour
{
    private List<Lobby> _lobbyList = new List<Lobby>();

    private void Start()
    {
        // 마우스 커서 강제 표시 및 잠금 해제
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnGUI()
    {
        // 버튼 크기 큼직하게 설정
        GUI.skin.button.fontSize = 30;
        GUI.skin.label.fontSize = 30;

        GUILayout.BeginArea(new Rect(10, 10, 400, 600));

        if (GUILayout.Button("1. 방 만들기 (Host)", GUILayout.Height(80)))
        {
            // "지훈이의 디펜스 방" 이라는 이름으로 방을 팝니다.
            _ = CloudLobbyManager.Instance.CreateLobby("지훈이의 디펜스 방"); 
        }

        if (GUILayout.Button("2. 방 목록 새로고침", GUILayout.Height(80)))
        {
            RefreshLobbyList();
        }

        GUILayout.Space(20);
        GUILayout.Label("--- [방 목록] ---");

        // 검색된 방들을 버튼으로 띄워줍니다. 누르면 바로 접속!
        foreach (Lobby lobby in _lobbyList)
        {
            if (GUILayout.Button($"[접속] {lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})", GUILayout.Height(60)))
            {
                _ = CloudLobbyManager.Instance.JoinLobby(lobby);
            }
        }

        GUILayout.EndArea();
    }

    private async void RefreshLobbyList()
    {
        _lobbyList = await CloudLobbyManager.Instance.GetLobbyList();
    }
}