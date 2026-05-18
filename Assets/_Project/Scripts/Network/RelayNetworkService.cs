using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayNetworkService : MonoBehaviour
{
    public static RelayNetworkService Instance { get; private set; }

    private void Awake() => SetSingleton();

    public async Task<string> StartHostWithRelayAsync(int maxConnections = 3)
    {
        try
        {
            // Relay 서버에 공간 할당
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            
            var serverEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
            var serverData = new RelayServerData(
                serverEndpoint.Host,
                (ushort)serverEndpoint.Port,
                allocation.AllocationIdBytes,
                allocation.ConnectionData,
                allocation.ConnectionData, // Host 자기 데이터를 두 번 넣음. 
                allocation.Key,
                serverEndpoint.Secure
            );

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            NetworkManager.Singleton.StartHost();
            
            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Relay] Host 시작 실패: {e.Message}");
            throw;
        }
    }

    public async Task StartClientWithRelayAsync(string joinCode)
    {
        try
        {
            // Join Code 로 Allocation 참가
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // 
            var serverEndpoint = joinAllocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
            var serverData = new RelayServerData(
                serverEndpoint.Host,
                (ushort)serverEndpoint.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData, // 방장의 데이터
                joinAllocation.Key,
                serverEndpoint.Secure
            );

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);
            NetworkManager.Singleton.StartClient();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Relay] Client 접속 실패: {e.Message}");
            throw;
        }
    }

    private void SetSingleton()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}