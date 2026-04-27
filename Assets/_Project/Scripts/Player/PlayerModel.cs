//　データ監視
public class PlayerModel
{
    //固定データ
    public float WalkSpeed { get; private set; } = 5.0f;
    public float RunSpeed { get; private set; } = 10.0f;
    public float JumpHeight { get; private set; } = 1.5f;
    public float Gravity { get; private set; } = -9.81f;
    
    public float DashSpeed { get; set; } = 20f;
    public float DashDuration { get; } = 0.25f; 
    // 変動するデータ
    public float VerticalVelocity { get; set; } = 0f;
    
    public float DashTimer { get; set; } = 0f;

    // 現在ダッシュ中かどうかを判定するなプロパティ
    public bool IsDashing => DashTimer > 0f;
}