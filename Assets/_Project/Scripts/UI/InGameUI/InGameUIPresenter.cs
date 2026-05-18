using UnityEngine;
using Unity.Netcode;

public class InGameUIPresenter : MonoBehaviour
{
    public static InGameUIPresenter Instance { get; private set; }
    [SerializeField] private InGameUIView _view;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _view.ShowHUDPanel(); // 인게임 시작 시 HUD 켜기
        
        // 메인 화면으로 돌아가기 버튼 로직 연결
        _view.OnReturnToLobbyClicked += HandleReturnToLobby;
    }

    // 🚨 플레이어(PlayerPresenter)가 스킬을 성공적으로 썼을 때 이 함수를 부릅니다!
    public void TriggerSkillCooldown(int skillSlot, float cooldownTime)
    {
        _view.StartCooldownUI(skillSlot, cooldownTime);
    }

    public void TriggerGameOver() 
    {
        _view.ShowGameOverPanel();
    }

    private async void HandleReturnToLobby()
    {
        // 🚨 씬 넘어가기 전에 풀링 시스템 청소!
        if (NetworkObjectPool.Instance != null)
        {
            NetworkObjectPool.Instance.ClearPool();
        }
        
        Debug.Log("로비로 돌아가는 중... 서버 데이터를 정리합니다.");

        // 1. 서버에 로비 삭제/퇴장 요청 (async 함수이므로 기다려줍니다)
        if (CloudLobbyManager.Instance != null)
        {
            await CloudLobbyManager.Instance.LeaveOrDeleteLobby();
        }

        // 2. 넷코드 셧다운
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // 3. 씬 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}