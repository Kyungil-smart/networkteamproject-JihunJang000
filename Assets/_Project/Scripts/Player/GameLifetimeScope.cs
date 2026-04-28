using UnityEngine;
using VContainer;
using VContainer.Unity;


// 사용할 순수 C# 클래스 생성(new Class 과정을 대신 수행), Inject할꺼들 등록.
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerView _playerView;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // injectの時でる部品登録
        builder.Register<InputReader>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<PlayerModel>(Lifetime.Scoped);
        builder.RegisterEntryPoint<PlayerPresenter>();
        builder.RegisterComponent(_playerView);
    }
}
