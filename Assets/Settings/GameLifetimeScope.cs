using VContainer;   
using VContainer.Unity;
using UnityEngine;
using UnityEditor.U2D.Sprites;

public class GameLifetimeScope : LifetimeScope
    {

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IAmapManager>(Lifetime.Singleton);
            builder.Register<IAmapManager>(Lifetime.Singleton);
            builder.Register<PlayerController>(Lifetime.Singleton);

            
        }
    }
