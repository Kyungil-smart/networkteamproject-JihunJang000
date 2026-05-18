using Unity.Netcode;
using UnityEngine;

public class IceOrbSkill : NetworkBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private float lifeTime = 2.0f; // 포탑 유지 시간 
    [SerializeField] private float fireRate = 0.4f; // 충돌체 발사 간격 
    
    [Header("Damage Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 15f; // 고드름이 날아가는 사거리
    [SerializeField] private float hitRadius = 1f; // 고드름의 굵기

    private float _lifeTimer;
    private float _fireTimer;
    private void OnEnable()
    {
        // 꼬리가 엉뚱한 곳에서부터 길게 늘어지는 것 방지
        TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>();
        foreach (var trail in trails)
        {
            trail.Clear();
        }

        // 허공에 남아있던 얼음 파티클 조각들을 청소하고 시작
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            particle.Clear();
            particle.Play();
        }
    }
    
    public override void OnNetworkSpawn()
    {
        // 풀에서 꺼낼 때마다 타이머 초기화
        _lifeTimer = lifeTime;
        _fireTimer = 0f; // 0으로 두면 소환되자마자 첫 발을 쏩니다.
    }

    private void Update()
    {
        if (!IsServer) return; // 데미지 판정은 방장만 계산

        // 수명 체크
        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
        {
            GetComponent<NetworkObject>().Despawn();
            return;
        }

        // 연사 속도에 맞춰 투명 충돌체 발사
        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            FireInvisibleHitbox();
            _fireTimer = fireRate; // 쿨타임 리셋
        }
    }

    private void FireInvisibleHitbox()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, hitRadius, transform.forward, range, LayerMask.GetMask("Enemy"));
        
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Nexus")) continue;
            
            if (hit.collider.TryGetComponent(out IDamageable enemy))
            {
                enemy.TakeDamage(damage);
                
                break; 
            }
        }
    }
}