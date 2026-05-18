using UnityEngine;

public class NexusHpBarPositioner : MonoBehaviour
{
    [Header("위치 설정")]
    public Transform nexusCenter;      // 넥서스 본체의 Transform
    public float heightOffset = 4f;    // 넥서스에서 얼마나 위로 띄울지
    public float forwardOffset = 2f;   // 플레이어쪽으로 얼마나 당겨올지

    private Camera _mainCamera;

    void Start()
    {
        // 씬에 있는 메인 카메라 찾아서 저장
        _mainCamera = Camera.main;
    }

    // 캐릭터들의 이동이 모두 끝난 후(Late)에 UI가 따라가야 덜덜 떨리지 않음. 
    void LateUpdate()
    {
        if (_mainCamera == null || nexusCenter == null) return;

        // 넥서스에서 카메라를 향하는 방향 계산 (높낮이는 무시하고 평면 방향만 계산)
        Vector3 dirToCamera = (_mainCamera.transform.position - nexusCenter.position);
        dirToCamera.y = 0; 
        dirToCamera.Normalize();

        // 넥서스 중심 + 위로 띄움 + 카메라 쪽으로 당겨옴
        transform.position = nexusCenter.position 
                             + (Vector3.up * heightOffset) 
                             + (dirToCamera * forwardOffset);

        // 회전관련.  텍스트나 이미지가 뒤집히지 않게 항상 카메라 화면과 평행하게
        transform.LookAt(transform.position + _mainCamera.transform.rotation * Vector3.forward,
            _mainCamera.transform.rotation * Vector3.up);
    }
}