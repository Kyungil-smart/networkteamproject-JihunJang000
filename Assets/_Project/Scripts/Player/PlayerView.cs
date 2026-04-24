using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private CharacterController _controller;

    // =>で実時間isGrounded数値反映
    public bool IsGrounded => _controller.isGrounded;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    // この関数を使ってPresenterから移動命令
    public void Move(Vector3 velocity)
    {
        _controller.Move(velocity * Time.deltaTime);
    }
}


