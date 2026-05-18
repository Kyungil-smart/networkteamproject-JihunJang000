using UnityEngine;
using Unity.Netcode;
public class Fireball : NetworkBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float damage = 25f;

    [SerializeField] private float lifeTime = 5f;
    private float _timer;
    public override void OnNetworkSpawn()
    {
        _timer = lifeTime; // 스폰될 때 타이머 초기화
    }

    private void Update()
    {
        // 서버가 아니거나, 아직 네트워크에 완전히 스폰되지 않았다면 무시
        if (!IsServer || !IsSpawned) return;
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // 서버에서만 투사체 수명 관리 및 Despawn 처리
        if (!IsServer) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            GetComponent<NetworkObject>().Despawn(); // 시간 지나면 풀로 반납
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌 파괴도 무조건 서버에서만 판정
        if (!IsServer || !IsSpawned) return;
        
        IDamageable target = other.GetComponent<IDamageable>();
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (target != null)
            {
                target.TakeDamage(damage);
                GetComponent<NetworkObject>().Despawn(); // 맞추면 풀로 반납
            }
        }
    }
}