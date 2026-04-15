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
            // Fungus 버전에 따라 Block은 GetComponents<Block>()으로 가져올 수 있습니다.
            var blocks = flowchart.GetComponents<Block>();
            foreach (var block in blocks)
            {
                if (block.CommandList == null) continue;

                foreach (var command in block.CommandList)
                {
                    // [수정 핵심]: 명령어가 null인 경우를 반드시 걸러내야 합니다.
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