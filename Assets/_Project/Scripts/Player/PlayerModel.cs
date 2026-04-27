//　データ監視
public class PlayerModel
{
    //固定データ
    public float MoveSpeed { get; private set; } = 5.0f;
    public float JumpHeight { get; private set; } = 1.5f;
    public float Gravity { get; private set; } = -9.81f;

    // 変動するデータ
    public float VerticalVelocity { get; set; } = 0f;
}