using UnityEngine;
using VContainer;
using VContainer.Unity;

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
