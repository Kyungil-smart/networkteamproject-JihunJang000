using Unity.Netcode;
using UnityEngine;

public class CloudRoomManager : NetworkBehaviour
{
    public static CloudRoomManager Instance { get; private set; }

    // 레디한 유저들의 ID를 담아두는 '네트워크 전용 리스트' (자동 동기화됨)
    public NetworkList<ulong> readyClients;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        readyClients = new NetworkList<ulong>();
    }

    // 유저가 UI에서 레디 버튼을 누르면 실행되는 함수 .
    public void ToggleReady()
    {
        ToggleReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    
    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(ulong clientId)
    {
        if (readyClients.Contains(clientId))
            readyClients.Remove(clientId); // 이미 레디 상태면 취소
        else
            readyClients.Add(clientId);    // 아니면 레디 완료 리스트에 추가
    }

    // 방장이 전부 레디했는지 검사하는 함수
    public bool IsAllReady()
    {
        if (!IsServer) return false;

        // 접속한 모든 사람을 한 명씩 검사
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) continue; // 방장 검사 패스
            if (!readyClients.Contains(clientId)) return false; // 한 명이라도 레디 안 했으면 안됨
        }
        return true;
    }

    // 방장이 시작 버튼을 누르면 실행
    public void StartGame()
    {
        if (IsServer && IsAllReady())
        {
            Debug.Log("🚀 전원 레디 완료! 게임 씬으로 이동합니다!");
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}