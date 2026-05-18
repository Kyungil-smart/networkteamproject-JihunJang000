using Unity.Netcode;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private CharacterController _controller;
    
    [Header("Projectile Settings")]
    [SerializeField] private GameObject _projectilePrefab; // 평타
    [SerializeField] private Transform _projectileSpawn;
    [SerializeField] private NetworkObjectPool _pool;
    
    [Header("Skill Prefabs (Network Objects)")]
    [SerializeField] private GameObject _skillQIceOrbPrefab; // Q스킬: 정면에 고정되는 고드름 포탑
    [SerializeField] private GameObject _skillEFireballPrefab; // E스킬: 날아가서 터지는 폭발물
    [SerializeField] private GameObject _skillRWavePrefab; // R스킬: 지진 파동
    
    private Animator _animator;
    private OwnerNetworkAnimator _netAnimator;
    
    public bool IsGrounded => _controller.isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _netAnimator = GetComponent<OwnerNetworkAnimator>();
    }

    [ServerRpc]
    public void FireProjectileServerRpc(Vector3 targetPoint)
    {
        if (_projectilePrefab == null || _projectileSpawn == null) 
        {
            return;
        }
        
        SpawnSkillProjectile(_projectilePrefab, targetPoint, 10f);
    }
    
    [ClientRpc]
    private void PlayAttackClientRpc()
    {
        // 시각 효과 나중에 
    }
    
    [ServerRpc]
    public void SkillQServerRpc() // Q는 정면 소환
    {
        if (_skillQIceOrbPrefab == null) return;

        // Q: 플레이어 정면 앞, 지면에서 약간 위에 아이스볼 소환
        Vector3 spawnPos = transform.position + (transform.forward * 2f) + Vector3.up; 
        
        NetworkObject netObj = NetworkObjectPool.Instance.GetNetworkObject(_skillQIceOrbPrefab, spawnPos, transform.rotation);
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }

    [ServerRpc]
    public void SkillEServerRpc(Vector3 targetPoint)
    {
        // E: 마우스 위치를 향해 날아가는 강력한 폭발물
        if (_skillEFireballPrefab != null)
        {
            SpawnSkillProjectile(_skillEFireballPrefab, targetPoint, 50f); 
        }
    }

    [ServerRpc]
    public void SkillRServerRpc(Vector3 targetPoint)
    {
        // R: 마우스 방향으로 1초간 전진하는 지진 파동 투사체 발사
        if (_skillRWavePrefab != null)
        {
            SpawnSkillProjectile(_skillRWavePrefab, targetPoint, 100f); 
        }
    }
    
    // 투사체 소환 공용 로직
    private void SpawnSkillProjectile(GameObject prefab, Vector3 target, float damage)
    {
        NetworkObject netObj = NetworkObjectPool.Instance.GetNetworkObject(prefab, _projectileSpawn.position, Quaternion.identity);
        
        // 풀에 없으면 에러 안 띄우고 중단하게 
        if (netObj == null)
        {
            Debug.LogError($"{prefab.name} 프리팹이 NetworkObjectPool에 등록되지 않았거나, NetworkObject가 없습니다!");
            return;
        }
        
        netObj.transform.LookAt(target);
        netObj.Spawn();
    }
    
    // 이동, 애니메이션 로직
    public void Move(Vector3 velocity)
    {
        _controller.Move(velocity * Time.deltaTime);
    }
    
    public void Rotate(Vector3 targetDirection, float rotationSpeed)
    {
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
    }
    
    public void SetMovingAnimation(bool isMoving) => _animator.SetBool("IsMoving", isMoving);
    public void SetSprintingAnimation(bool isSprinting) => _animator.SetBool("IsSprinting", isSprinting);
    public void TriggerDashAnimation() => _animator.SetTrigger("DashTrigger");
    public void SetGroundedState(bool isGrounded) => _animator.SetBool("IsGrounded", isGrounded);
    public void SetActionSpeed(float speedMultiplier) => _animator.SetFloat("ActionSpeed", speedMultiplier);
    
    public void TriggerAttack() => PlayActionAnim("BaseAttack"); 
    public void TriggerSkillQ() => PlayActionAnim("Q");
    public void TriggerSkillE() => PlayActionAnim("E");
    public void TriggerSkillR() => PlayActionAnim("R");
    
    private void PlayActionAnim(string stateName)
    {
        // 자신 화면에서는 파라미터 무시하고 즉시 해당 애니메이션으로 0.1초만에 넘어가기. 
        _animator.CrossFade(stateName, 0.1f, 0, 0f);

        if (IsServer) 
        {
            PlayAnimClientRpc(stateName); 
        }
        else 
        {
            PlayAnimServerRpc(stateName); 
        }
    }

    [ServerRpc]
    private void PlayAnimServerRpc(string stateName)
    {
        PlayAnimClientRpc(stateName);
    }

    [ClientRpc]
    private void PlayAnimClientRpc(string stateName)
    {
        if (IsOwner) return; // 나 자신은 무시

        if (_animator != null)
        {
            // 다른 사람 화면에서도 강제로 애니메이션 상태 변경
            _animator.CrossFade(stateName, 0.1f, 0, 0f);
        }
    }
    
}