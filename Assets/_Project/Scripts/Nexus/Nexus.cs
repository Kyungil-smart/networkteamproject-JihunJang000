using Unity.Netcode;
using UnityEngine;

public class Nexus : NetworkBehaviour, IDamageable
{
    [Header("넥서스 설정")]
    public float maxHp = 500f;

    // 체력바 UI 스크립트를 연결
    [Header("UI 설정")]
    public HpUI nexusHpUI;

    //  NetworkVariable을 쓰면 서버에서 깎은 체력이 모든 클라이언트에게 자동 동기화
    public NetworkVariable<float> currentHp = new NetworkVariable<float>(
        500f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server // 체력을 깎는 권한은 서버(호스트)에게만 줌
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHp.Value = maxHp; // 게임 시작 시 체력 꽉 채우기
        }

        // 시작할 때 체력바 초기화
        if (nexusHpUI != null)
        {
            nexusHpUI.ResetHpBar();
        }
        
        // 체력 값이 변할 때마다 모든 클라이언트 화면에서 자동으로 실행되는 이벤트
        currentHp.OnValueChanged += (oldValue, newValue) =>
        {
            Debug.Log($"[넥서스] 현재 체력: {newValue} / {maxHp}");

            // 체력이 깎일 때마다 HpUI의 갱신 함수를 호출(3초 지속) 
            if (nexusHpUI != null)
            {
                nexusHpUI.UpdateHpBar(newValue, maxHp);
            }
            
            if (newValue <= 0f)
            {
                GameOver();
            }
            
        };
        
    }

    public void TakeDamage(float damage)
    {
        // 데미지 판정은 서버만 할 수 있도록 함
        if (!IsServer || currentHp.Value <= 0f) return;

        currentHp.Value -= damage;

        // 체력이 0 이하로 떨어지면 게임 오버
        if (currentHp.Value <= 0f)
        {
            currentHp.Value = 0f;
        }
    }

    private void GameOver()
    {
        Debug.LogError("넥서스 파괴됨 게임 오버 ");
        
        // MVP 패턴의 Presenter를 호출하여 게임 오버 View
        if (InGameUIPresenter.Instance != null)
        {
            InGameUIPresenter.Instance.TriggerGameOver();
        }
    }
}