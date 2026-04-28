using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField]  private CharacterController _controller;

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


