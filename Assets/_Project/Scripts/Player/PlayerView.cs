using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField]  private CharacterController _controller;
    
    [Header("Projetile Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _projectileSpawn;
    
    private Animator _animator;
    // =>で実時間isGrounded数値反映。マップが平面でCharacterControllerのisGrounded使用
    public bool IsGrounded => _controller.isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    // この関数を使ってPresenterからview移動命令
    public void Move(Vector3 velocity)
    {
        _controller.Move(velocity * Time.deltaTime);
    }
    
    public void Rotate(Vector3 targetDirection, float rotationSpeed)
    {
        // 回転値計算
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        // Slerp移動
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
    }
    
    public void FireProjectile(Vector3 targetPoint)
    {
        if (_projectilePrefab == null || _projectileSpawn == null) 
        {
            Debug.LogWarning("파이어볼 프리팹이나 FirePoint가 연결되지 않았습니다!");
            return;
        }

        // FirePoint 위치에 파이어볼 생성
        GameObject fireball = Instantiate(_projectilePrefab, _projectileSpawn.position, Quaternion.identity);
        
        // 생성된 파이어볼의 고개를 타겟 좌표로 돌림 (Vector3.forward로 이동하므로 고개만 돌림) 
        fireball.transform.LookAt(targetPoint);
    }
    
    // 애니메이션 세팅
    public void SetMovingAnimation(bool isMoving) => _animator.SetBool("IsMoving", isMoving);
    public void SetSprintingAnimation(bool isSprinting) => _animator.SetBool("IsSprinting", isSprinting);
    public void TriggerDashAnimation() => _animator.SetTrigger("DashTrigger");
    public void SetGroundedState(bool isGrounded) => _animator.SetBool("IsGrounded", isGrounded);
    // 애니메이션 속도 설정.
    public void SetActionSpeed(float speedMultiplier) => _animator.SetFloat("ActionSpeed", speedMultiplier);
    
    public void TriggerAttack() => _animator.SetTrigger("AttackTrigger");
    public void TriggerSkillQ() => _animator.SetTrigger("SkillQTrigger");
    public void TriggerSkillE() => _animator.SetTrigger("SkillETrigger");
    public void TriggerSkillR() => _animator.SetTrigger("SkillRTrigger");
    
    
    
}


