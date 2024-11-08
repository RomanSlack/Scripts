using System.Collections;
using UnityEngine;

public class Tool_Conversation : MonoBehaviour
{
    public int maxTurns = 10; // Number of exchanges before ending the conversation

    public IEnumerator StartConversation(AgentBehavior agent1, AgentBehavior agent2)
    {
        int turnCount = 0;
        bool agent1Turn = true;

        while (turnCount < maxTurns)
        {
            string message = agent1Turn ? $"{agent1.agentName}'s turn to speak" : $"{agent2.agentName}'s turn to speak";
            Debug.Log(message);

            // Simulate a message exchange
            if (agent1Turn)
            {
                yield return agent1.StartCoroutine(agent1.AskOllama("Generate a response for conversation"));
                agent2.ShowDialogue($"{agent1.agentName}: {agent1.GetCurrentResponse()}");
            }
            else
            {
                yield return agent2.StartCoroutine(agent2.AskOllama("Generate a response for conversation"));
                agent1.ShowDialogue($"{agent2.agentName}: {agent2.GetCurrentResponse()}");
            }

            agent1Turn = !agent1Turn;
            turnCount++;
            yield return new WaitForSeconds(1f); // Optional delay between turns
        }

        Debug.Log("Conversation ended.");
    }
}
