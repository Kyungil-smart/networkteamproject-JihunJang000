using UnityEngine;
using VContainer;
using VContainer.Unity;

// MonoBehaviourを継承せず、VContainerのインターフェースでライフサイクルを管理
public class PlayerPresenter : IStartable, ITickable
{
    private readonly IInputProvider _input;
    private readonly PlayerView _view;
    private readonly PlayerModel _model;

    // 変更: MonoBehaviourではないので [SerializeField] は不要です
    private Transform _cameraTransform;
    
    // 必要な部品を外部から注入 (DI)
    [Inject]
    public PlayerPresenter(IInputProvider input, PlayerView view, PlayerModel model)
    {
        _input = input;
        _view = view;
        _model = model;
    }

    // GameLifetimeScopeのEntryPointで StartとTick使用可能
    public void Start() 
    {
        // イベントの購読（Jump, Dash）
        _input.OnJumpEvent += WhenJump;
        _input.OnDashEvent += WhenDash; // 追加: ダッシュイベント
        
        _cameraTransform = Camera.main.transform;
    }

    // VContainer使う時のupdate
    public void Tick()
    {
        // 重力はダッシュ中・スタン中に関わらず常に計算する
        ApplyGravity();

        // 状態確認：ダッシュ中の場合は通常移動をスキップ
        if (_model.IsDashing)
        {
            ApplyDashMovement();
            return;
        }

        // 通常の移動処理
        ApplyMovement();
    }
    
    

    // ダッシュ実行中の強制移動処理
    private void ApplyDashMovement()
    {
        // タイマーを減らす
        _model.DashTimer -= Time.deltaTime;

        // カメラ方向ではなく、キャラクターの正面(forward)へ高速移動
        Vector3 dashVelocity = _view.transform.forward * _model.DashSpeed + (Vector3.up * _model.VerticalVelocity);
        
        _view.Move(dashVelocity);
    }

    // 通常の移動とスプリント処理
    private void ApplyMovement()
    {
        
        Vector2 inputDir = _input.MoveDirection;
        
        // 変更: Presenterのフィールド変数ではなく、ローカル変数として処理
        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        
        // 追加: スプリント判定 (移動中 かつ Shiftキー押下)
        bool isSprinting = isMoving && _input.IsSprinting;
        
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        
        // Y軸の反映を消す（水平移動のみにする）
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize(); // カメラの前方向
        camRight.Normalize();
        
        Vector3 moveDir = (camForward * inputDir.y + camRight * inputDir.x).normalized;
        
        if (moveDir != Vector3.zero)
        {
            // 回転処理
            _view.Rotate(moveDir, Time.deltaTime * 60f); 
        }

        // 追加: スプリント状態に応じて速度を切り替える
        float currentSpeed = isSprinting ? _model.RunSpeed : _model.WalkSpeed;
        
        Vector3 finalVelocity = (moveDir * currentSpeed) + (Vector3.up * _model.VerticalVelocity);
        
        // アニメーション
        _view.SetMovingAnimation(isMoving);
        _view.SetSprintingAnimation(isSprinting);
        _view.SetGroundedState(_view.IsGrounded);
        
        // 最終的な移動命令
        _view.Move(finalVelocity);
    }

    
    private void ApplyGravity()
    {
        // 落下速度の計算
        if (_view.IsGrounded && _model.VerticalVelocity < 0)
        {
            _model.VerticalVelocity = -2f; 
        }
        _model.VerticalVelocity += _model.Gravity * Time.deltaTime;
    }

    // ダッシュイベント発火時の処理
    private void WhenDash()
    {
        // 既にダッシュ中、または動けない状態なら無視
        if (_model.IsDashing) return;

        // Modelのタイマーにダッシュの持続時間をセット
        _model.DashTimer = _model.DashDuration;
        _view.TriggerDashAnimation();
    }
    private void WhenJump()
    {
        // 追加: ダッシュ中はジャンプできないように制限
        if (_view.IsGrounded && !_model.IsDashing)
        {
            _model.VerticalVelocity = Mathf.Sqrt(_model.JumpHeight * -2f * _model.Gravity);
            _view.SetGroundedState(_view.IsGrounded);
        }
    }
    
    
    
}