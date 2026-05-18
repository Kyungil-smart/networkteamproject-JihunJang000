using Unity.Netcode;
using UnityEngine;
using VContainer;

// NGO의 스폰 콜백을 받고 MVP를 조립하는 역할
public class PlayerBootstrapper : NetworkBehaviour
{
    [SerializeField] private PlayerView _view; 
    private PlayerPresenter _presenter;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var scope = Object.FindFirstObjectByType<GameLifetimeScope>();
            if (scope != null)
            {
                IInputProvider input = scope.Container.Resolve<IInputProvider>();
                
                PlayerModel model = new PlayerModel();
                
                _presenter = new PlayerPresenter(input, _view, model);
                _presenter.Start();
            }
        }
    }
    private void Update()
    {
        if (IsOwner && _presenter != null)
        {
            _presenter.Tick();
        }
    }
}