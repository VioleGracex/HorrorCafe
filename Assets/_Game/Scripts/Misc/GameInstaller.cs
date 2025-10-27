using Zenject;
using Ouiki.UI;
using UnityEngine;

public class GameInstaller : MonoInstaller
{
    [SerializeField] ReadableCanvasManager readableCanvasManager;

    public override void InstallBindings()
    {
        Container.Bind<ReadableCanvasManager>().FromInstance(readableCanvasManager).AsSingle();
    }
}