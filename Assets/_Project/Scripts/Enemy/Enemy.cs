using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

// 적의 상태 정의
public enum EnemyState { Move, Attack, Die }

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : NetworkBehaviour, IDamageable
{
    [Header("적 설정")]
    public float maxHp = 50f;
    public float currentHp;
    public float attackRange = 2.5f; // 넥서스 공격 사거리
    public float attackDamage = 5f;  // 넥서스에 입히는 데미지
    public float attackCooldown = 1.5f; // 공격 속도
    public float nexusRadius = 1.5f; // 넥서스를 포위할 때 띄울 간격 (반지름)

    [Header("UI 설정")] 
    public GameObject damageTextPrefab;
    public HpUI hpUI; // 적 머리 위의 체력바 스크립트 연결용
    
    private NavMeshAgent _agent;
    private Transform _targetNexus;
    private Animator _anim;
    
    private EnemyState _currentState;
    private float _lastAttackTime;
    private Vector3 _myDestination; // 계산된 넥서스 표면의 목표 좌표

    public override void OnNetworkSpawn()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();

        transform.rotation = Quaternion.identity;
        
        if (hpUI != null)
        {
            hpUI.ResetHpBar();
        }
        
        
        // 클라이언트/서버 물리엔진 분리
        if (!IsServer)
        {
            // 클라이언트  비활성화 
            if (_agent != null) _agent.enabled = false;
            
            // 클라이언트의 물리 엔진을 강제로 꺼서 서버 위치 따르게 함. 
            if (TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            if (TryGetComponent(out CharacterController cc)) cc.enabled = false;
            
            return;
        }

        // 서버에선 물리 엔진과 길찾기를 정상적으로 키기 
        if (TryGetComponent(out Rigidbody serverRb)) serverRb.isKinematic = false;
        if (TryGetComponent(out CharacterController serverCc)) serverCc.enabled = true;

        // 풀링 핵심: 스폰될 때마다 체력과 상태를 새것으로 최고하 
        currentHp = maxHp;
        if (_agent != null) _agent.enabled = true; 
        

        GameObject nexus = GameObject.FindGameObjectWithTag("Nexus");
        if (nexus != null)
        {
            _targetNexus = nexus.transform;

            // 포위망 길찾기 로직
            Vector3 dirFromNexusToMe = (transform.position - _targetNexus.position).normalized;
            dirFromNexusToMe.y = 0f;
            dirFromNexusToMe.Normalize();
            _myDestination = _targetNexus.position + (dirFromNexusToMe * nexusRadius);

            ChangeState(EnemyState.Move); // 태어나자마자 이동 상태로 전환
        }
        else
        {
            Debug.LogError("적: 넥서스를 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        if (!IsServer) return; // 모든 AI 판단은 서버만

        switch (_currentState)
        {
            case EnemyState.Move:
                HandleMoveState();
                break;
            case EnemyState.Attack:
                HandleAttackState();
                break;
            case EnemyState.Die:
                // 죽었을 때는 아무 행동도 하지 않음
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        _currentState = newState;

        // 상태에 따라 길찾기를 켜고 끄기
        if (_agent != null && _agent.isOnNavMesh)
        {
            if (newState == EnemyState.Attack || newState == EnemyState.Die)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero; // 미끄러지는 관성까지 강제로 0으로 뺌
            }
            else if (newState == EnemyState.Move)
            {
                _agent.isStopped = false;
            }
        }

        
        ChangeAnimationClientRpc(newState);
    }

    private void HandleMoveState()
    {
        if (_targetNexus == null) return;

        float distance = Vector3.Distance(transform.position, _myDestination);

        // 넥서스에 충분히 가까워지면 공격 상태로 전환
        if (distance <= attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        else
        {
            _agent.SetDestination(_myDestination); 
        }
    }

    private void HandleAttackState()
    {
        if (_targetNexus == null) return;

        // 쿨타임이 찰 때마다 공격
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            Debug.Log($"넥서스를 공격합니다 데미지: {attackDamage}");
            
            // 넥서스에게 데미지 전달
            IDamageable nexusDamageable = _targetNexus.GetComponent<IDamageable>();
            if (nexusDamageable != null)
            {
                nexusDamageable.TakeDamage(attackDamage);
            }

            // 1.5초마다 반복되는 공격 애니메이션도 접속자들에게서 재생
            TriggerAttackAnimClientRpc();
            
            _lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer || _currentState == EnemyState.Die) return; 

        currentHp -= damage;

        // 데미지 텍스트 띄우기
        if (damageTextPrefab != null && NetworkObjectPool.Instance != null)
        {
            var textObj = NetworkObjectPool.Instance.GetNetworkObject(
                damageTextPrefab, 
                transform.position + Vector3.up * 2f, 
                Quaternion.identity
            );
            textObj.Spawn(); 

            if (textObj.TryGetComponent(out DamageText dmgText))
            {
                dmgText.SetDamageTextClientRpc(damage);
            }
        }

        UpdateHpBarClientRpc(currentHp, maxHp);
        
        if (currentHp <= 0f)
        {
            ChangeState(EnemyState.Die);
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("적 처치됨 -> 사망 애니메이션 후 풀로 반납");
        Invoke(nameof(DespawnEnemy), 1.5f); 
    }

    private void DespawnEnemy()
    {
        if (TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
            netObj.Despawn(); 
    }

    
    // 네트워크 동기화 전용
    [ClientRpc]
    private void UpdateHpBarClientRpc(float current, float max)
    {
        if (hpUI != null) hpUI.UpdateHpBar(current, max);
    }

    [ClientRpc]
    private void ChangeAnimationClientRpc(EnemyState newState)
    {
        if (_anim != null)
        {
            _anim.SetBool("IsMoving", newState == EnemyState.Move);
            if (newState == EnemyState.Attack) _anim.SetTrigger("DoAttack");
            if (newState == EnemyState.Die) _anim.SetTrigger("DoDie");
        }
    }

    [ClientRpc]
    private void TriggerAttackAnimClientRpc()
    {
        if (_anim != null) _anim.SetTrigger("DoAttack");
    }
}