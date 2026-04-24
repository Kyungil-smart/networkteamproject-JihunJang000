using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private InputActions _inputActions;
    private CharacterController _characterController;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f; // rotation speed

    // gravity process variables
    private float _verticalVelocity;
    private float _gravity = -9.81f;

    private void Awake()
    {
        _inputActions = new InputActions();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable() => _inputActions.Player.Enable();
    private void OnDisable() => _inputActions.Player.Disable();

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        // New Input System에서 WASD 입력값(Vector2) 읽어오기
        Vector2 inputVector = _inputActions.Player.Move.ReadValue<Vector2>();
        
        // 2. 3D 공간의 이동 벡터로 변환 (X축은 좌우, Z축은 앞뒤)
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        // 3. 입력이 있을 때만 이동 및 회전 처리
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            // 부드럽게 바라보는 방향 회전 (Slerp 사용)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. 중력 적용 (공중에 떠있지 않게 바닥으로 끌어당김)
        if (_characterController.isGrounded)
        {
            _verticalVelocity = -2f; // 땅에 붙어있도록 약간의 힘을 줌
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime; // 공중에 있으면 중력 가속
        }

        // 최종 이동 벡터에 중력 값 합치기
        moveDirection.y = _verticalVelocity;

        // 5. CharacterController를 통해 실제 이동 적용
        // 방향 * 속도 * 프레임보정(Time.deltaTime)
        _characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
    }
}