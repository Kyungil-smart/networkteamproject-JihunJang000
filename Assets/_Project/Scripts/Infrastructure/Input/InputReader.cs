using System;
using UnityEngine;
using UnityEngine.InputSystem;

// input system DIのキーボードService部品。

// 현재의 값을 계속 반영하는 것은 ReadValue, IsPressed를 사용, 단발성은 performed 사용해서 읽기. 
// 이벤트의 경우 performed += 함수(이벤트 invoke)로 물리적 버튼이랑 이벤트 실행 연결.
// 이벤트에 함수 추가.(presenter에서 함수 추가하면, 버튼 눌릴때 해당함수 발동)

// Monobehaviour가 아닌 순수 C# 클래스로, 선언 시 유니티 시스템에 영향 받지 않고 유지.(싱글톤 느낌인데 조금 더 최적화 됨)
// 순수 C#에서 구독 등록 해제하기 위해서 IDisposable 계승
public class InputReader : IInputProvider, IDisposable
{
    // "generate C# class"で自動生成したInput ActionsのC#クラス。
    private InputActions _inputActions;

    // 現在の入力値
    public Vector2 MoveDirection => _inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookDirection => _inputActions.Player.Look.ReadValue<Vector2>();

    public bool IsSprinting => _inputActions.Player.Sprint.IsPressed();

    // アクションのイベント定義（単純なボータン達）
    public event Action OnJumpEvent; 
    public event Action OnBasicAttackEvent;
    public event Action OnSkillQEvent;
    public event Action OnSkillEEvent;
    public event Action OnSkillREvent;
    public event Action OnDashEvent;

    public InputReader()
    {
        _inputActions = new InputActions();
        
        _inputActions.Player.Enable();

        // input keyにコールバック関数登録
        _inputActions.Player.Jump.performed += OnJump;
        _inputActions.Player.BasicAttack.performed += OnBasicAttack;
        _inputActions.Player.SkillQ.performed += OnSkillQ;
        _inputActions.Player.SkillE.performed += OnSkillE;
        _inputActions.Player.SkillR.performed += OnSkillR;
        _inputActions.Player.Dash.performed += OnDash;
    }

    // 各コールバック関数
    private void OnJump(InputAction.CallbackContext context) => OnJumpEvent?.Invoke();
    private void OnBasicAttack(InputAction.CallbackContext context) => OnBasicAttackEvent?.Invoke();
    private void OnSkillQ(InputAction.CallbackContext context) => OnSkillQEvent?.Invoke();
    private void OnSkillE(InputAction.CallbackContext context) => OnSkillEEvent?.Invoke();
    private void OnSkillR(InputAction.CallbackContext context) => OnSkillREvent?.Invoke();
    private void OnDash(InputAction.CallbackContext context) => OnDashEvent?.Invoke();

    // 解放すつ時メモリ最適化　（disableする時event同録解除）
    public void Dispose()
    {
        _inputActions.Player.Jump.performed -= OnJump;
        _inputActions.Player.BasicAttack.performed -= OnBasicAttack;
        _inputActions.Player.SkillQ.performed -= OnSkillQ;
        _inputActions.Player.SkillE.performed -= OnSkillE;
        _inputActions.Player.SkillR.performed -= OnSkillR;
        _inputActions.Player.Dash.performed -= OnDash;
        
        _inputActions.Player.Disable();
    }
}