using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CloudLobbyManager : MonoBehaviour
{
    public static CloudLobbyManager Instance { get; private set; }

    private Lobby _joinedLobby;
    private float _heartbeatTimer;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }
    
    // 유니티 로비는 30초 동안 아무 연락이 없으면 폭파되어 방장이 15초마다 방 생존 신호를 송신
    private async void HandleLobbyHeartbeat()
    {
        if (_joinedLobby != null && _joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            _heartbeatTimer -= Time.deltaTime;
            if (_heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                _heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
            }
        }
    }

    
    // 방 만들기 (Host)
    
    public async Task CreateLobby(string lobbyName, int maxPlayers = 3)
    {
        try
        {
            Debug.Log("방 생성 중... 릴레이 개통 중...");
            
            // 아까 만든 Relay 스크립트에서 입장 코드를 뽑아옵니다.
            string relayJoinCode = await RelayNetworkService.Instance.StartHostWithRelayAsync(maxPlayers);
            
            // 방 설정 (릴레이 코드를 RelayJoinCode라고 이름 붙인뒤 숨겨둠)
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };

            // 로비 서버에 방 생성
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log($"방 생성 완료 . 방 이름: {_joinedLobby.Name} / 릴레이 코드: {relayJoinCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"방 생성 실패: {e.Message}");
        }
    }
    
    // 방 목록 가져오기
    public async Task<List<Lobby>> GetLobbyList()
    {
        try
        {
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log($"🔍 현재 만들어진 방 개수: {response.Results.Count}개");
            return response.Results;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ 방 목록 가져오기 실패: {e.Message}");
            return new List<Lobby>();
        }
    }

    
    // 방 접속하기 (Client)
    public async Task JoinLobby(Lobby lobby)
    {
        try
        {
            Debug.Log($"방 접속 시도 중... ID: {lobby.Id}");
        
            // 로비 게시판을 통해 방 입장
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
        }
        catch (LobbyServiceException e)
    {
        // 유니티 공식 Enum 'LobbyConflict' 사용 
        //  에러 메시지에 "already a member"가 포함되어 있는지 체크 해서 
        if (e.Reason == LobbyExceptionReason.LobbyConflict && e.Message.Contains("already a member"))
        {
            Debug.LogWarning("이미 로비 멤버입니다. 재연결을 시도");
            try
            {
                // Join 대신 Reconnect를 사용하여 현재 접속 중인 로비 정보를 강제로 가져옴 
                _joinedLobby = await LobbyService.Instance.ReconnectToLobbyAsync(lobby.Id);
            }
            catch (Exception reconnectEx)
            {
                Debug.LogError($"재연결 실패: {reconnectEx.Message}");
                return;
            }
        }
        else
        {
            // 그 외의 로비 서비스 에러 처리
            Debug.LogError($"❌ 로비 접속 중 에러 발생: {e.Message}");
            return;
        }
    }
        catch (Exception e)
        {
            // 로비 외의 일반 예외 처리
            Debug.LogError($"❌ 예기치 못한 에러: {e.Message}");
            return;
        }

        // 로비 접속(재연결 포함)에 성공했다면, 이후 릴레이 로직을 실행
        try
        {
            if (_joinedLobby != null && _joinedLobby.Data.ContainsKey("RelayJoinCode"))
            {
                string relayJoinCode = _joinedLobby.Data["RelayJoinCode"].Value;
                Debug.Log($"로비 데이터 확보 완료 릴레이 코드로 연결 시도: {relayJoinCode}");

                await RelayNetworkService.Instance.StartClientWithRelayAsync(relayJoinCode);
            }
            else
            {
                Debug.LogError("로비 데이터에 RelayJoinCode가 없습니다!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"릴레이 연결 실패: {e.Message}");
        }
    }
    
    // 로비 삭제 또는 퇴장
    public async Task LeaveOrDeleteLobby()
    {
        if (_joinedLobby == null) return;

        try
        {
            string playerId = AuthenticationService.Instance.PlayerId;

            // 내가 방장인 경우 로비 자체를 폭파
            if (_joinedLobby.HostId == playerId)
            {
                await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
                Debug.Log("방장이 로비를 삭제했습니다");
            }
            // 내가 Client인 경우 나만 로비에서 퇴장
            else
            {
                await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
                Debug.Log("클라이언트가 로비에서 퇴장했습니다");
            }

            // 변수 초기화
            _joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"로비 정리 중: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"로비 정리 중 에러: {e.Message}");
        }
    }

    // 게임이 강제로 꺼지거나 에디터 정지 시 유령 방 방지
    private async void OnApplicationQuit()
    {
        // 종료될 때 서버에 퇴장/삭제 신호를 보내고 제거
        await LeaveOrDeleteLobby();
    }
}