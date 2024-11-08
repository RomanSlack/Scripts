using System.Collections;
using UnityEngine;

public class Tool_Conversation : MonoBehaviour
{
    public int conversationRounds = 5;  // Number of exchanges between agents

    // Start a conversation between two agents
    public void StartConversation(AgentBehavior agent1, AgentBehavior agent2)
    {
        if (agent1 != null && agent2 != null)
        {
            StartCoroutine(HandleConversation(agent1, agent2));
        }
        else
        {
            Debug.LogError("One or both agents are missing for the conversation.");
        }
    }

    // Coroutine to handle conversation exchanges
    private IEnumerator HandleConversation(AgentBehavior agent1, AgentBehavior agent2)
    {
        // Custom initial prompts to set the context and start the conversation
        string initialPrompt1 = "You see a fellow villager approaching. This is a casual conversation, so say something to start it off about something that interests you. Keep it brief, like 2-3 sentences.";
        string initialPrompt2 = "A familiar villager greets you. They’re interested in talking, so respond as if you're in a real conversation. You live in a small village and know each other. Keep responses brief, no more than 3 sentences.";

        // Start the conversation by having agent1 initiate it with their interests
        yield return agent1.GenerateResponse(initialPrompt1);
        yield return new WaitForSeconds(1.0f); // Delay for natural pacing

        // Set up the alternating conversation rounds
        AgentBehavior currentSpeaker = agent2;
        AgentBehavior listener = agent1;

        for (int i = 0; i < conversationRounds; i++)
        {
            // The listener replies based on the last statement of the current speaker
            yield return currentSpeaker.GenerateResponse(listener.GetLastResponse());

            // Swap roles for the next round
            AgentBehavior temp = currentSpeaker;
            currentSpeaker = listener;
            listener = temp;

            yield return new WaitForSeconds(1.0f); // Delay for pacing
        }

        Debug.Log("Conversation complete between " + agent1.agentName + " and " + agent2.agentName);
    }
}
