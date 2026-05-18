using Unity.Netcode;
using UnityEngine;
using TMPro;

public class DamageText : NetworkBehaviour
{
    [Header("폰트 설정")]
    public TextMeshProUGUI textMesh; 
    public float moveSpeed = 2f;    // 위로 올라가는 속도
    public float fadeSpeed = 2f;    // 사라지는 속도
    public float lifetime = 1.0f;   // 화면에 머무는 시간

    private float _timer;
    private Color _originalColor;
    private Camera _mainCamera;

    private void Awake()
    {
        if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();
        _originalColor = textMesh.color;
    }

    public override void OnNetworkSpawn()
    {
        _mainCamera = Camera.main; // 폰트가 나(플레이어)를 바라보게 하기 위해 카메라 찾기
        _timer = lifetime;
        textMesh.color = _originalColor; // 풀에서 꺼낼 때마다 색상(투명도) 원상복구!
    }

    private void Update()
    {
        // 1. 항상 카메라(플레이어) 쪽을 바라보게 회전 (빌보드 효과)
        if (_mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
        }

        // 2. 위로 둥실둥실 이동
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 3. 서서히 투명해지기 (Alpha 값 줄이기)
        _timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(_timer / lifetime);
        textMesh.color = new Color(_originalColor.r, _originalColor.g, _originalColor.b, alpha);

        if (!IsServer || !IsSpawned) return;
        
        // 4. 수명이 다 되면 풀(Pool)로 반납! (서버에서만 파괴 권한 가짐)
        if (IsServer && _timer <= 0f)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    // 💡 방장(서버)이 "숫자 바꿔!"라고 명령하면 모든 클라이언트의 화면에 반영됨
    [ClientRpc]
    public void SetDamageTextClientRpc(float damageAmount)
    {
        textMesh.text = $"-{damageAmount}";
    }
}