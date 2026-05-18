using Unity.Netcode;
using UnityEngine;

public class ExplosiveFireball : NetworkBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float explosionRadius = 4f; // 광역 폭발 반경
    [SerializeField] private float lifeTime = 5f;

    [Header("Visual Effects")]
    // 이펙트를 PlayerView가 아닌 투사체가 적용시킴 
    [SerializeField] private GameObject _bigExplosionPrefab; 

    private float _timer;
    private bool _hasExploded = false; // 중복 폭발 방지용

    public override void OnNetworkSpawn()
    {
        _timer = lifeTime;
        _hasExploded = false;
        
        // 재활옹
        if (TryGetComponent(out MeshRenderer mesh)) mesh.enabled = true;
        if (TryGetComponent(out Collider col)) col.enabled = true;
    }
    private void Update()
    {
        // 파이어볼 전진
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (!IsServer) return;

        // 수명 체크
        _timer -= Time.deltaTime;
        if (_timer <= 0f && !_hasExploded)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 서버에서만, 그리고 아직 안 터졌을 때만 폭발 로직 실행
        if (!IsServer || _hasExploded) return;
        
        Explode();
    }

    // 핵심 폭발 로직 (서버에서 실행)
    private void Explode()
    {
        _hasExploded = true; // 락을 걸어서 두 번 터지는 것 방지

        // OverlapSphere로 반경 내의 모든 적을 찾음
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Enemy"));
        
        // 데미지 적용
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IDamageable target)) target.TakeDamage(damage);
        }

        // 폭발 이펙트 띄우기 
        SpawnExplosionEffectClientRpc(transform.position);

        // 즉시 삭제하지 않고 투사체를 안 보이게만 만든 뒤 0.1초 뒤에 삭제
        // 이펙트 생성 명령이 클라이언트에게 도착할 시간을 벌어줌
        if (TryGetComponent(out MeshRenderer mesh)) mesh.enabled = false;
        if (TryGetComponent(out Collider col)) col.enabled = false;
        
        Invoke(nameof(DelayedDespawn), 0.1f);
    }

    private void DelayedDespawn()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    // 폭발 시각 효과 생성 (클라이언트에서 실행)
    [ClientRpc]
    private void SpawnExplosionEffectClientRpc(Vector3 explosionPos)
    {
        if (_bigExplosionPrefab != null)
        {
            // 충돌한 위치에 BigExplosion 파티클 생성
            GameObject fx = Instantiate(_bigExplosionPrefab, explosionPos, Quaternion.identity);
            Destroy(fx, 3f); // 3초 뒤 파티클 찌꺼기 삭제
        }
    }
}