using UnityEngine;
using VContainer;
using VContainer.Unity;

// MonoBehaviourを継承せず、VContainerのインターフェースでライフサイクルを管理

// 플레이어 MVP의 Presenter. 네트워크 연결된 변수들 결합도를 낮춰서 네트워크 작업전 플레이어 작업을 하기 위함.
// 움직임 계산해서 Model에 저장. View에 반영.
public class PlayerPresenter : IStartable, ITickable
{
    private readonly IInputProvider _input;
    private readonly PlayerView _view;
    private readonly PlayerModel _model;
    
    private Camera _mainCamera;
    
    [Inject]
    public PlayerPresenter(IInputProvider input, PlayerView view, PlayerModel model)
    {
        _input = input;
        _view = view;
        _model = model;
    }

    public void Start() 
    {
        // 카메라 세팅은 Owner일 때만 수행
        if (_view.IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            _mainCamera = Camera.main;
            
            // 시네머신 타겟을 나로 설정
            var freeLookCamera = Object.FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
            if (freeLookCamera != null)
            {
                Transform cameraFocus = _view.transform.Find("Camera_Focus");
                freeLookCamera.Target.TrackingTarget = cameraFocus != null ? cameraFocus : _view.transform;
            }
            
            // 내 캐릭터일 때만 입력 이벤트를 구독
            _input.OnJumpEvent += WhenJump;
            _input.OnDashEvent += WhenDash; 
            _input.OnBasicAttackEvent += WhenAttack;
            _input.OnSkillQEvent += WhenSkillQ;
            _input.OnSkillEEvent += WhenSkillE;
            _input.OnSkillREvent += WhenSkillR;
        }
    }

    public void Tick()
    {
        // 도플갱어 방지 코드. 내 캐릭터가 아니면 로직을 실행X
        if (!_view.IsOwner) return;

        ApplyGravity();
        UpdateTimers();
        
        if (_model.IsDashing)
        {
            ApplyDashMovement();
            return;
        }
        
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
        if (_model.IsActing)
        {
            // 공중에 있을 때를 대비해 중력만 적용하고 제자리에 멈춤
            Vector3 gravityOnly = Vector3.up * _model.VerticalVelocity;
            _view.Move(gravityOnly);
            
            // 이동 애니메이션도 강제로 종료 
            _view.SetMovingAnimation(false); 
            _view.SetSprintingAnimation(false);
            return; // 아래쪽의 WASD 이동 로직을 실행하지 않고 함수를 빠져나갑니다.
        }
        
        Vector2 inputDir = _input.MoveDirection;
        
        bool isMoving = inputDir.sqrMagnitude > 0.01f;
        
        bool isSprinting = isMoving && _input.IsSprinting;
        
        Vector3 camForward = _mainCamera.transform.forward;
        Vector3 camRight = _mainCamera.transform.right;
        
        // Y軸の反映を消す
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
        if (_view.IsGrounded && !_model.IsDashing)
        {
            _model.VerticalVelocity = Mathf.Sqrt(_model.JumpHeight * -2f * _model.Gravity);
            _view.SetGroundedState(_view.IsGrounded);
        }
    }
    
    // 스킬들 쿨타임 계산. 
    private void UpdateTimers()
    {
        if (_model.ActionTimer > 0f) _model.ActionTimer -= Time.deltaTime;
        
        if (_model.AttackTimer > 0f) _model.AttackTimer -= Time.deltaTime;
        if (_model.SkillQTimer > 0f) _model.SkillQTimer -= Time.deltaTime;
        if (_model.SkillETimer > 0f) _model.SkillETimer -= Time.deltaTime;
        if (_model.SkillRTimer > 0f) _model.SkillRTimer -= Time.deltaTime;
    }
    
    // --- 스킬 관련 수행 ---
    private void WhenAttack()
    {
        // 쿨타임이 다 찼을 때만 실행
        if (!_model.IsActing && _model.CanAttack) // 쿨타임 다찼을떄, 스킬 사용 중이 아닐떄
        {
            //캐릭터 방향 전환 
            Vector3 camForward = _mainCamera.transform.forward;
            camForward.y = 0f; // 몸 기울지 않게 y는 0
            _view.transform.forward = camForward.normalized;
            
            _model.AttackTimer = _model.AttackCooldown; // 타이머 리셋
            _model.ActionTimer = _model.AttackIngameSpeed; //애니메이션 지속시간.동안 다른행동 불가.
            
            
            float speed = _model.AttackAnimLength / _model.AttackIngameSpeed;
            _view.SetActionSpeed(speed);
            _view.TriggerAttack(); // 애니메이션 재생
            
            Vector3 targetPoint = CalculateTargetPoint(); // 화면의 정중앙 계산.
            _view.FireProjectileServerRpc(targetPoint); // 정중앙에 투사체 발사. 
        }
    }
    
    
    // 정중앙 계산 및 타겟 위치 반환. 
    private Vector3 CalculateTargetPoint()
    {
        // 화면의 정중앙 좌표
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);

        LayerMask.GetMask("Enemy");
        
        // 최대 사거리 100f 안에 적 맞으면 위치 반환 
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Enemy")))
        {
            return hit.point;
        }
        // 허공에 쏠때 100 앞 지점 반환. 
        return ray.GetPoint(20f);
    }
    
    private void WhenSkillQ() 
    {
        if (!_model.IsActing && _model.CanUseQ)
        {
            FaceToCamera();

            _model.SkillQTimer = _model.SkillQCooldown;
            _model.ActionTimer = _model.SkillQIngameSpeed;
            
            float speed = _model.SkillQAnimLength / _model.SkillQIngameSpeed;
            _view.SetActionSpeed(speed);
            _view.TriggerSkillQ();
            
            Vector3 targetPoint = CalculateTargetPoint();
            _view.SkillQServerRpc();

            // UI 매니저에게 Q 쿨타임 연출 시작 명령
            if (InGameUIPresenter.Instance != null) 
                InGameUIPresenter.Instance.TriggerSkillCooldown(0, _model.SkillQCooldown);
        }
    }

    private void WhenSkillE() 
    {
        if (!_model.IsActing && _model.CanUseE)
        {
            FaceToCamera();

            _model.SkillETimer = _model.SkillECooldown;
            _model.ActionTimer = _model.SkillEIngameSpeed;
            
            float speed = _model.SkillEAnimLength / _model.SkillEIngameSpeed;
            _view.SetActionSpeed(speed);
            _view.TriggerSkillE();
            
            Vector3 targetPoint = CalculateTargetPoint();
            _view.SkillEServerRpc(targetPoint);
            
            if (InGameUIPresenter.Instance != null) 
                InGameUIPresenter.Instance.TriggerSkillCooldown(1, _model.SkillECooldown);
        }
    }

    private void WhenSkillR()
    {
        if (!_model.IsActing && _model.CanUseR)
        {
            Vector3 groundPoint = CalculateGroundPoint();

            _model.SkillRTimer = _model.SkillRCooldown;
            _model.ActionTimer = _model.SkillRIngameSpeed;

            float speed = _model.SkillRAnimLength / _model.SkillRIngameSpeed;
            _view.SetActionSpeed(speed);
            _view.TriggerSkillR();

            _view.SkillRServerRpc(groundPoint);
            
            if (InGameUIPresenter.Instance != null)
                InGameUIPresenter.Instance.TriggerSkillCooldown(2, _model.SkillRCooldown);
        }
    }

    // 카메라 정면으로 캐릭터 돌리기
    private void FaceToCamera()
    {
        Vector3 camForward = _mainCamera.transform.forward;
        camForward.y = 0f;
        _view.transform.forward = camForward.normalized;
    }

    // R스킬용 바닥 좌표 구하기
    private Vector3 CalculateGroundPoint()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        // 허공을 바라보고 썼을 경우, 캐릭터 앞 5m 지점을 기본값으로 반환
        return _view.transform.position + (_view.transform.forward * 5f); 
    }
    
}