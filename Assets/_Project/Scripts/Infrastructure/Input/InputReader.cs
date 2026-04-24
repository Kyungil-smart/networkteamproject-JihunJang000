using System;
using UnityEngine;
using UnityEngine.InputSystem;

// input system DIのキーボードService部品。
public class InputReader : IInputProvider, IDisposable
{
    // "generate C# class"で自動生成したInput ActionsのC#クラス。
    private InputActions _inputActions;

    // 現在の入力値
    public Vector2 MoveDirection => _inputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookDirection => _inputActions.Player.Look.ReadValue<Vector2>();

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