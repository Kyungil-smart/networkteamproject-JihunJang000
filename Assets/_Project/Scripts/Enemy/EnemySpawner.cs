using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab;    // 소환할 적 프리팹
    public Transform[] spawnPoints;   // 적이 튀어나올 위치들
    public float spawnInterval = 3f;  // 몇 초마다 생성할지
    public NetworkObjectPool pool; 
    
    
    public override void OnNetworkSpawn()
    {
        // 서버만 생성 가능. 
        if (IsServer)
        {
            StartCoroutine(SpawnEnemyRoutine());
        }
    }

    private IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (!IsServer) yield break;

            // 풀링에서 현재 필드에 나가 있는 적의 숫자를 가져옴
            if (pool.GetCurrentActiveCount(enemyPrefab) >= pool.GetMaxSize(enemyPrefab))
            {
                Debug.LogWarning("적 생성 상한 도달! 생성을 건너뜁니다.");
                continue; // 이번 루프는 생성하지 않고 다음 간격까지 대기
            }

            Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var enemyNetObj = pool.GetNetworkObject(enemyPrefab, randomPoint.position, randomPoint.rotation);
            enemyNetObj.Spawn(); 
        }
    }
}