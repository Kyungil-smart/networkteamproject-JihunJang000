using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField]  private CharacterController _controller;

    // =>で実時間isGrounded数値反映
    public bool isGrounded => _controller.isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    // この関数を使ってPresenterからview移動命令
    public void Move(Vector3 velocity)
    {
        _controller.Move(velocity * Time.deltaTime);
    }
    
    public void Rotate(Vector3 targetDirection, float rotationSpeed)
    {
        // 回転値計算
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        // Slerpを使って、現在の回転からターゲットの回転へ滑らかに補間
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed);
    }
}


