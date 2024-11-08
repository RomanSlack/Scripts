using UnityEngine;
using System.Collections.Generic;

public class Checker : MonoBehaviour
{
    public float detectionRadius = 3f; // Radius within which agents are considered "nearby"
    private List<AgentBehavior> agents = new List<AgentBehavior>();

    private void Start()
    {
        agents.AddRange(FindObjectsOfType<AgentBehavior>()); // Register all agents in the scene
    }

    public List<AgentBehavior> GetNearbyAgents(AgentBehavior requestingAgent)
    {
        List<AgentBehavior> nearbyAgents = new List<AgentBehavior>();

        foreach (AgentBehavior agent in agents)
        {
            if (agent != requestingAgent) // Exclude the agent making the request
            {
                float distance = Vector3.Distance(agent.transform.position, requestingAgent.transform.position);
                if (distance <= detectionRadius)
                {
                    nearbyAgents.Add(agent);
                }
            }
        }

        return nearbyAgents;
    }
}
