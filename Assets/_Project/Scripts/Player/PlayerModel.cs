//　データ監視

// 데이터 선언. 
// 고정적인 데이터는 프로퍼티 private set;. 모델은 Monobehaviour 상속을 못받기에 인스펙터 창에서 변수 조절 불가. 
public class PlayerModel
{
    //固定データ
    public float WalkSpeed { get; private set; } = 5.0f; // 나중에 변동 가능할 수도?
    public float RunSpeed { get; set; } = 10.0f;
    public float JumpHeight { get; private set; } = 1.5f;
    public float Gravity { get; private set; } = -9.81f;
    public float DashSpeed { get; private set; } = 20f;

    public float DashDuration { get; private set; } = 0.25f;

    // 変動するデータ
    public float VerticalVelocity { get; set; } = 0f;

    public float DashTimer { get; set; } = 0f;

    // 現在ダッシュ中かどうかを判定するなプロパティ
    // 변수 참조 시 대쉬중인지 실시간 체크
    public bool IsDashing => DashTimer > 0f; //읽기 전용. get DashTimer > 0이랑 비슷. 

    // -- 스킬 관련 변수 -- 
    public float AttackCooldown { get; } = 0.5f; // 평타 간격
    public float SkillQCooldown { get; } = 3.0f; // Q 쿨타임
    public float SkillECooldown { get; } = 8.0f; 
    public float SkillRCooldown { get; } = 15.0f;
    
    // 스킬 애니메이션 실제 속도
    public float AttackAnimLength { get; } = 10/24f; 
    public float SkillQAnimLength { get; } = 18/24f;
    public float SkillEAnimLength { get; } = 18/24f;
    public float SkillRAnimLength { get; } = 23/24f;

    public float AttackIngameSpeed { get; set; } = 0.6f;
    public float SkillQIngameSpeed { get; set;} = 1f;
    public float SkillEIngameSpeed { get; set;} = 1f;
    public float SkillRIngameSpeed { get; set;} = 1.5f;

    // --- 스킬별 타이머 ---
    public float AttackTimer { get; set; } = 0f;
    public float SkillQTimer { get; set; } = 0f;
    public float SkillETimer { get; set; } = 0f;
    public float SkillRTimer { get; set; } = 0f;

    // --- 상태 체크 프로퍼티 ---
    public bool CanAttack => AttackTimer <= 0f;
    public bool CanUseQ => SkillQTimer <= 0f;
    public bool CanUseE => SkillETimer <= 0f;
    public bool CanUseR => SkillRTimer <= 0f;

    // 공격 중인지 체크 (공격도중에는 다른 스킬, 평타들 발동 안됨)
    public float ActionTimer { get; set; } = 0f; // 공격 애니메이션 지속 시간. 
    public bool IsActing => ActionTimer > 0f;

}