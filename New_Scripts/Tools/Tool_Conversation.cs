using UnityEngine;

public class Tool_Conversation : MonoBehaviour
{
    public void InitiateConversation(AgentBehavior agent)
    {
        Debug.Log($"{agent.agentName} is starting a conversation based on the LLM's decision.");
        // Add your conversation handling logic here.
    }
}
