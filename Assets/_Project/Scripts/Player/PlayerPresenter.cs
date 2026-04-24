using UnityEngine;
using VContainer;
using VContainer.Unity;

// MonoBehaviourを継承せず、VContainerのインターフェースでライフサイクルを管理
public class PlayerPresenter : IStartable, ITickable
{
    private readonly IInputProvider _input;
    private readonly PlayerView _view;
    private readonly PlayerModel _model;

    // コンストラクタ・インジェクション（必要な部品を外部から注入）
    [Inject]
    public PlayerPresenter(IInputProvider input, PlayerView view, PlayerModel model)
    {
        _input = input;
        _view = view;
        _model = model;
    }

    public void Start()
    {
        // ジャンプイベントの購読
        _input.OnJumpEvent += HandleJump;
    }

    public void Tick()
    {
        ApplyMovement();
    }

    private void ApplyMovement()
    {
        // 1. 入力値の取得と方向ベクトルの生成
        Vector2 inputDir = _input.MoveDirection;
        Vector3 moveDir = new Vector3(inputDir.x, 0f, inputDir.y).normalized;

        // 2. 重力と接地時の浮遊バグ防止処理（Modelのデータを更新）
        if (_view.IsGrounded && _model.VerticalVelocity < 0)
        {
            _model.VerticalVelocity = -2f; 
        }
        _model.VerticalVelocity += _model.Gravity * Time.deltaTime;

        // 3. 最終的な移動ベクトルの計算（Modelの速度データを適用）
        Vector3 finalVelocity = (moveDir * _model.MoveSpeed) + (Vector3.up * _model.VerticalVelocity);
        
        // 4. Viewへ移動命令を下す
        _view.Move(finalVelocity);
    }

    private void HandleJump()
    {
        // 気絶しておらず、かつ接地している場合のみジャンプ可能
        if (_view.IsGrounded)
        {
            _model.VerticalVelocity = Mathf.Sqrt(_model.JumpHeight * -2f * _model.Gravity);
        }
    }
}