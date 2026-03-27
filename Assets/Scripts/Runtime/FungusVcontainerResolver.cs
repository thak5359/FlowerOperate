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
        // 씬 내의 모든 Flowchart를 찾습니다.
        var flowcharts = Object.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
        
        foreach (var flowchart in flowcharts)
        {
            // Flowchart 내의 모든 Block을 가져옵니다.
            var blocks = flowchart.GetComponents<Block>();
            foreach (var block in blocks)
            {
                // 각 Block 내의 모든 Command에 의존성을 주입합니다.
                foreach (var command in block.CommandList)
                {
                    _container.Inject(command);
                }
            }
        }
        
        Debug.Log("Fungus Commands에 모든 의존성 주입 완료!");
    }
}