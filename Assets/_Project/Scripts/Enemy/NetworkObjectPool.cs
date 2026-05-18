using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class NetworkObjectPool : NetworkBehaviour
{
    [System.Serializable]
    public struct PoolConfig
    {
        public GameObject prefab;
        public int defaultCapacity;
        public int maxSize;
    }

    public List<PoolConfig> poolConfigs = new List<PoolConfig>();
    
    // IObjectPool 대신 ObjectPool<NetworkObject>를 사용
    private Dictionary<GameObject, ObjectPool<NetworkObject>> _pools = new Dictionary<GameObject, ObjectPool<NetworkObject>>();
    private Dictionary<NetworkObject, GameObject> _instanceToPrefab = new Dictionary<NetworkObject, GameObject>();

    public static NetworkObjectPool Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public override void OnNetworkSpawn()
    {
        foreach (var config in poolConfigs)
        {
            if (config.prefab == null) continue;

            var prefab = config.prefab;
            
            // 프리팹이 NetworkObject를 가지고 있는지 확인
            if (!prefab.TryGetComponent<NetworkObject>(out _))
            {
                Debug.LogError($"{prefab.name}에 NetworkObject 컴포넌트가 없습니다");
                continue;
            }

            var pool = new ObjectPool<NetworkObject>(
                createFunc: () => Instantiate(prefab).GetComponent<NetworkObject>(),
                actionOnGet: (obj) => obj.gameObject.SetActive(true),
                actionOnRelease: (obj) => obj.gameObject.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj.gameObject),
                defaultCapacity: config.defaultCapacity,
                maxSize: config.maxSize
            );
            
            _pools.Add(prefab, pool);

            // NGO 핸들러 등록
            NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledNetworkPrefabHandler(prefab, this));
        }
    }

    public override void OnDestroy()
    {
        // 싱글톤 찌꺼기 지우기
        if (Instance == this) 
        {
            Instance = null;
        }

        // 네트워크 매니저가 살아있다면, 내 Handler를 명부에서 다 지움
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.PrefabHandler != null)
        {
            foreach (var config in poolConfigs)
            {
                if (config.prefab != null)
                {
                    NetworkManager.Singleton.PrefabHandler.RemoveHandler(config.prefab);
                }
            }
        }
        
        base.OnDestroy(); // 원래 있던 파괴 로직 실행 
    }
    
    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!_pools.ContainsKey(prefab)) return null;

        NetworkObject obj = null;

        // 살아있는 오브젝트를 뽑을 때까지 계속 반복
        while (true)
        {
            // 풀에서 꺼냅니다. (창고 안에 없으면 새로 생성 )
            obj = _pools[prefab].Get(); 

            // Unity 엔진에서 진짜로 존재하는지 체크
            if (obj != null && obj.gameObject != null)
            {
                break; // 정상적인 오브젝트 발견시 
            }
            else
            {
                // 파괴된게 적 오브젝트라면 가비지 컬렉터가 치움
            }
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        _instanceToPrefab[obj] = prefab;
        return obj;
    }

    public void ReturnNetworkObject(NetworkObject netObj)
    {
        // 씬 전환 등으로 이미 파괴된 오브젝트를 창고에 다시 넣으려 하는 행위를 금지 
        if (netObj == null || netObj.gameObject == null) return;

        if (_instanceToPrefab.TryGetValue(netObj, out var prefab))
        {
            _pools[prefab].Release(netObj);
        }
    }
    
    public void ClearPool()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Clear(); // UnityEngine.Pool 라이브러리 자체의 청소 기능
        }
        _instanceToPrefab.Clear();
        _pools.Clear(); // 딕셔너리까지 비워줘서 다음 게임에 OnNetworkSpawn에서 새로 생성
        Debug.Log("풀링 시스템을 초기화했습니다.");
    }

    public int GetCurrentActiveCount(GameObject prefab)
    {
        if (_pools.TryGetValue(prefab, out var pool))
        {
            return pool.CountActive;
        }
        return 0;
    }

    public int GetMaxSize(GameObject prefab)
    {
        var config = poolConfigs.Find(x => x.prefab == prefab);
        return config.maxSize;
    }
}


public class PooledNetworkPrefabHandler : INetworkPrefabInstanceHandler
{
    private GameObject _prefab;
    private NetworkObjectPool _pool;

    public PooledNetworkPrefabHandler(GameObject prefab, NetworkObjectPool pool)
    {
        _prefab = prefab;
        _pool = pool;
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        return _pool.GetNetworkObject(_prefab, position, rotation);
    }

    public void Destroy(NetworkObject networkObject)
    {
        _pool.ReturnNetworkObject(networkObject);
    }
}