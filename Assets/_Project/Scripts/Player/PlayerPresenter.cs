using UnityEngine;
using VContainer;
using VContainer.Unity;

// MonoBehaviourを継承せず、VContainerのインターフェースでライフサイクルを管理
public class PlayerPresenter : IStartable, ITickable
{
    private readonly IInputProvider _input;
    private readonly PlayerView _view;
    private readonly PlayerModel _model;

    [SerializeField] private Transform _cameraTransform;
    
    // 必要な部品を外部から注入 (コンストラクタでInject）
    [Inject]
    public PlayerPresenter(IInputProvider input, PlayerView view, PlayerModel model)
    {
        _input = input;
        _view = view;
        _model = model;
    }

    public void Start()
    {
        _input.OnJumpEvent += HandleJump;

        _cameraTransform = Camera.main.transform;
    }

    //　VContainer使う時のupdate.
    public void Tick()
    {
        ApplyMovement();
    }
    
    private void ApplyMovement()
    {
        Vector2 inputDir = _input.MoveDirection;
        
        Vector3 camForward = _cameraTransform.forward;
        Vector3 camRight = _cameraTransform.right;
        
        //Y軸反映 X 
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize(); //カメラの前方向
        camRight.Normalize();
        
        Vector3 moveDir = (camForward * inputDir.y + camRight * inputDir.x).normalized;
        
        if (moveDir != Vector3.zero)
        {
            _view.Rotate(moveDir, Time.deltaTime * 60f); 
            
        }
        if (_view.isGrounded && _model.VerticalVelocity < 0)
        {
            _model.VerticalVelocity = -2f; 
        }
        _model.VerticalVelocity += _model.Gravity * Time.deltaTime;
        
        
        
        Vector3 finalVelocity = (moveDir * _model.MoveSpeed) + (Vector3.up * _model.VerticalVelocity);
        
        _view.Move(finalVelocity);
    }

    private void HandleJump()
    {
        if (_view.isGrounded)
        {
            _model.VerticalVelocity = Mathf.Sqrt(_model.JumpHeight * -2f * _model.Gravity);
        }
    }
}