using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyAndRoomUI : MonoBehaviour
{
    private List<Lobby> _lobbyList = new List<Lobby>();

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnGUI()
    {
        GUI.skin.button.fontSize = 25;
        GUI.skin.label.fontSize = 25;

        GUILayout.BeginArea(new Rect(20, 20, 500, 800));

        // 넷코드 연결 상태에 따라 화면을 설정. 
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            DrawLobbyUI(); // 연결 안 됨 -> 로비 게시판 보여주기
        else
            DrawRoomUI();  // 연결 됨 -> 대기실 화면 보여주기

        GUILayout.EndArea();
    }
    
    // 로비 게시판
    private void DrawLobbyUI()
    {
        GUILayout.Label("🏁 [로비 게시판]");
        if (GUILayout.Button("1. 방 만들기 (Host)", GUILayout.Height(60)))
            _ = CloudLobbyManager.Instance.CreateLobby("지훈이의 디펜스 방");

        if (GUILayout.Button("2. 방 목록 새로고침", GUILayout.Height(60)))
            RefreshLobbyList();

        GUILayout.Space(20);
        GUILayout.Label("--- [현재 있는 방 목록] ---");
        foreach (Lobby lobby in _lobbyList)
        {
            if (GUILayout.Button($"[입장] {lobby.Name} ({lobby.Players.Count}/{lobby.MaxPlayers})", GUILayout.Height(50)))
                _ = CloudLobbyManager.Instance.JoinLobby(lobby);
        }
    }

    // ==========================================
    // [화면 2] 대기실 (Room)
    // ==========================================
    private void DrawRoomUI()
    {
        GUILayout.Label("🏠 [대기실에 입장했습니다!]");
        GUILayout.Space(10);
        
        if (CloudRoomManager.Instance == null || !CloudRoomManager.Instance.IsSpawned) return;

        GUILayout.Label($"현재 참가 인원: {NetworkManager.Singleton.ConnectedClientsIds.Count}명");
        
        // 1. 접속한 유저 목록과 레디 상태 출력
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            bool isHost = (clientId == NetworkManager.ServerClientId);
            bool isReady = CloudRoomManager.Instance.readyClients.Contains(clientId);
            string myTag = (clientId == NetworkManager.Singleton.LocalClientId) ? " (나)" : "";
            
            string status = isHost ? "👑 방장" : (isReady ? "✅ 준비 완료!" : "❌ 대기 중...");
            GUILayout.Label($"유저 {clientId}{myTag} : {status}");
        }

        GUILayout.Space(30);

        // 2. 접속자(Client) 화면: 레디 버튼
        if (!NetworkManager.Singleton.IsServer)
        {
            bool amIReady = CloudRoomManager.Instance.readyClients.Contains(NetworkManager.Singleton.LocalClientId);
            string btnText = amIReady ? "레디 취소" : "🔥 준비 (Ready)!";
            
            if (GUILayout.Button(btnText, GUILayout.Height(80)))
                CloudRoomManager.Instance.ToggleReady();
        }
        // 3. 방장(Host) 화면: 게임 시작 버튼
        else
        {
            if (CloudRoomManager.Instance.IsAllReady() && NetworkManager.Singleton.ConnectedClientsIds.Count > 1)
            {
                if (GUILayout.Button("🚀 게임 시작 (Start)!", GUILayout.Height(80)))
                    CloudRoomManager.Instance.StartGame();
            }
            else
            {
                GUILayout.Label("⏳ 모든 유저가 레디해야 시작할 수 있습니다.");
                
                // (선택) 혼자 개발 테스트할 때 쓰기 위한 강제 시작 버튼
                if (GUILayout.Button("혼자 테스트용 강제 시작", GUILayout.Height(40)))
                    CloudRoomManager.Instance.StartGame();
            }
        }
    }

    private async void RefreshLobbyList()
    {
        _lobbyList = await CloudLobbyManager.Instance.GetLobbyList();
    }
}