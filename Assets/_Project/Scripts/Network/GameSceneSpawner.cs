using Unity.Netcode;
using UnityEngine;

public class GameSceneSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    private void Start()
    {
        // 씬이 켜졌을 때 코드를 실행하는 사람이 서버인지 확인
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            // 방장이 GameScene에 도착했서 방장 캐릭부터 소환.  
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);

            // 로비에서 GameScene으로 넘어오는 다른 클라이언트들을 기다렸다가 스폰해줌. 
            NetworkManager.Singleton.SceneManager.OnLoadComplete += (clientId, sceneName, mode) =>
            {
                // 방장이 아닌 다른 사람이 GameScene 로딩을 끝낸경우. 
                if (sceneName == "GameScene" && clientId != NetworkManager.Singleton.LocalClientId)
                {
                    SpawnPlayer(clientId);
                }
            };
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        // 지정된 유저의 권한(clientId)을 주고 캐릭터를 소환
        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        Debug.Log($"{clientId}번 유저 캐릭터 소환");
    }
}