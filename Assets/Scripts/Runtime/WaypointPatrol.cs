using UnityEngine;
using UnityEngine.AI;
public class WaypointPatrol : MonoBehaviour
{

    public NavMeshAgent navMeshAgent;
    public Transform[] waypoints;

    int m_currentWaypointIndex;

    void Start()
    {
        if (navMeshAgent != null || waypoints.Length > 0)
            navMeshAgent.SetDestination(waypoints[0].position);
        else
        {
            Debug.Log("navMeshAgent or waypoint is Null!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            m_currentWaypointIndex = (m_currentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.SetDestination(waypoints[m_currentWaypointIndex].position);
        }
    }
}
