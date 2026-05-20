using VContainer;
using VContainer.Unity;
using Fungus;
using UnityEngine;

public class FungusDependencyResolver : IStartable
{
    private readonly IObjectResolver _container;

    public FungusDependencyResolver(IObjectResolver container)
    {
        _container = container;
    }

    public void Start()
    {
        var flowcharts = Object.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);

        foreach (var flowchart in flowcharts)
        {
            var blocks = flowchart.GetComponents<Block>();
            foreach (var block in blocks)
            {
                if (block.CommandList == null) continue;

                foreach (var command in block.CommandList)
                {
                    if (command != null)
                    {
                        _container.Inject(command);
                    }
                }
            }
        }

        Debug.Log("Fungus Commands에 모든 의존성 주입 완료!");
    }
}