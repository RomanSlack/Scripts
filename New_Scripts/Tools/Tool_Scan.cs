using System.Collections.Generic;
using UnityEngine;

public class Tool_Scan : MonoBehaviour
{
    private Checker checker;

    private void Awake()
    {
        checker = FindObjectOfType<Checker>(); // Find the Checker instance in the scene
    }

    public List<AgentBehavior> ExecuteScan(AgentBehavior agent)
    {
        List<AgentBehavior> nearbyAgents = checker.GetNearbyAgents(agent);
        if (nearbyAgents.Count > 0)
        {
            Debug.Log($"{agent.agentName} found {nearbyAgents.Count} nearby agents.");
        }
        else
        {
            Debug.Log($"{agent.agentName} found no nearby agents.");
        }
        
        return nearbyAgents;
    }
}
