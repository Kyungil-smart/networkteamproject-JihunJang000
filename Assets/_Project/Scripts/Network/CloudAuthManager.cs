using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class CloudAuthManager : MonoBehaviour
{
    // 씬이 시작되면 로그인을 시도합니다.
    async void Start()
    {
        await AuthenticateToUnityCloud();
    }

    private async Task AuthenticateToUnityCloud()
    {
        try
        {
            // 유니티 클라우드 서비스 엔진 켜기
            await UnityServices.InitializeAsync();
            Debug.Log("️유니티 서비스 초기화");

            // 익명 로그인
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                
                // 로그인 성공시 고유 ID 디버그 출력
                Debug.Log($"익명 로그인 성공 . 내 고유 ID: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (Exception e)
        {
            // 통신 에러가 날떄
            Debug.LogError($"접속 실패:  {e.Message}");
        }
    }
}