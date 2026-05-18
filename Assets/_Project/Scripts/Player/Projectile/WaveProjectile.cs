using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WaveProjectile : NetworkBehaviour
{
    [SerializeField] private float speed = 15f; // 1초에 이동할 거리 
    [SerializeField] private float damage = 100f;
    
    private float _timer = 1f; // 딱 1초 동안만 유지
    private HashSet<Collider> _alreadyHit = new HashSet<Collider>(); // 이미 때린 적 기억하는 집합 

    public override void OnNetworkSpawn()
    {
        _timer = 1f; 
        _alreadyHit.Clear(); 
    }
    
    private void Update()
    {
        if (!IsServer || !IsSpawned) return;
        
        // 파동 전진
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (!IsServer) return;

        // 1초 뒤 자동 소멸
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; 

        // 이미 맞은 적이면 무시하고 통과
        if (_alreadyHit.Contains(other)) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (other.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(damage);
                _alreadyHit.Add(other); // 맞은 적 명단에 추가
            }
        }
    }
}