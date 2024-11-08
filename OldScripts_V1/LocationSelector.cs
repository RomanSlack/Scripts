using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LocationSelector : MonoBehaviour
{
    public OllamaManager agent1Ollama;
    public OllamaManager agent2Ollama;
    public OllamaManager godAgentOllama;  // God agent to evaluate conversation
    public ConversationManager conversationManager;
    public LocationTool locationTool;
    public PlayerPathfinding agent1Pathfinding;
    public PlayerPathfinding agent2Pathfinding;

    private Dictionary<string, Vector3> locations = new Dictionary<string, Vector3>
    {
        { "park", new Vector3(5, 3, 0) },
        { "gym", new Vector3(-8, 4.8f, 0) },
        { "cafe", new Vector3(-8, -6f, 0) },
        { "home", new Vector3(11, -6f, 0) }
    };

    private bool negotiationInProgress = false;
    private bool conversationEnded = false;  // Track if the conversation has ended
    private bool agentsMoved = false;  // Ensure agents only move once

    private string agent1Response = "";
    private string agent2Response = "";

    private void Start()
    {
        ValidateComponents();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !negotiationInProgress && !conversationEnded)
        {
            StartLocationNegotiation();
        }
    }

    private void ValidateComponents()
    {
        if (agent1Ollama == null) Debug.LogError("Agent 1's OllamaManager is not assigned!");
        if (agent2Ollama == null) Debug.LogError("Agent 2's OllamaManager is not assigned!");
        if (godAgentOllama == null) Debug.LogError("God agent's OllamaManager is not assigned!");
        if (conversationManager == null) Debug.LogError("ConversationManager is not assigned!");
        if (locationTool == null) Debug.LogError("LocationTool is not assigned!");
        if (agent1Pathfinding == null || agent2Pathfinding == null)
            Debug.LogError("Pathfinding components are not assigned!");
    }

    private void StartLocationNegotiation()
    {
        Debug.Log("Starting location negotiation.");
        negotiationInProgress = true;
        conversationEnded = false;
        agentsMoved = false;
        conversationManager.ClearConversations();  // Reset the conversation

        string initialPrompt = 
            "You are going to be in a simulated environment where you can make decisions and interact with other people. " +
            "Keep your responses short and to the point. Respond with a location by placing the location at the end of the sentence. " +
            "Decide where to meet for tea: park, gym, cafe, or home. Both must agree to proceed. " +
            "Once you agree, end with 'final choice [location]'.";

        conversationManager.AddConversation("System", "Prompting Alice to select a location...");
        StartCoroutine(agent1Ollama.AskOllama(initialPrompt, HandleAgent1Response, gameObject));
    }

    private void HandleAgent1Response(string response, GameObject agent)
    {
        if (conversationEnded) return;  // Prevent further processing after conversation ends

        conversationManager.AddConversation("Alice", $"Alice: {response}");
        agent1Response = response;

        string promptForBob = 
            $"You are Bob. Alice said:\n\n'{response}'. " +
            "Do you agree, or suggest a different location? Confirm with 'Location: [your choice]'.";

        conversationManager.AddConversation("System", "Prompting Bob to respond...");
        StartCoroutine(agent2Ollama.AskOllama(promptForBob, HandleAgent2Response, gameObject));
    }

    private void HandleAgent2Response(string response, GameObject agent)
    {
        if (conversationEnded) return;  // Prevent further processing after conversation ends

        conversationManager.AddConversation("Bob", $"Bob: {response}");
        agent2Response = response;

        conversationManager.AddConversation("System", "Evaluating responses with God agent...");
        StartCoroutine(EvaluateWithGodAgent());
    }

    private IEnumerator EvaluateWithGodAgent()
    {
        string conversationLog = $"Alice: {agent1Response}\nBob: {agent2Response}";
        string godPrompt = 
            $"Based on this conversation:\n\n{conversationLog}\n\n" +
            "Do Alice and Bob agree on a location? If yes, extract the agreed location and respond with 'final choice [location]'. " +
            "If no, respond with 'no agreement'.";

        yield return StartCoroutine(godAgentOllama.AskOllama(godPrompt, HandleGodAgentResponse, gameObject));
    }

    private void HandleGodAgentResponse(string response, GameObject agent)
    {
        if (response.ToLower().Contains("no agreement"))
        {
            RestartNegotiation();
        }
        else
        {
            string agreedLocation = ExtractLocationFromResponse(response);
            if (IsValidLocation(agreedLocation))
            {
                ExecuteMove(agreedLocation);
            }
            else
            {
                Debug.LogError($"God agent provided an invalid location: {response}");
                RestartNegotiation();
            }
        }
    }

    private void ExecuteMove(string locationKey)
    {
        if (agentsMoved) return;  // Prevent multiple moves

        if (locations.TryGetValue(locationKey.ToLower().Trim(), out Vector3 newPosition))
        {
            conversationManager.AddConversation("System", $"Both agents agreed on {locationKey}. Moving...");

            agent1Pathfinding.target.position = newPosition;
            agent2Pathfinding.target.position = newPosition;

            agent1Pathfinding.StartPathfinding();
            agent2Pathfinding.StartPathfinding();

            EndConversation();
        }
        else
        {
            Debug.LogError($"Invalid location key: {locationKey}");
            RestartNegotiation();
        }
    }

    private void EndConversation()
    {
        Debug.Log("Conversation ended. Agents moved to the selected location.");
        conversationManager.AddConversation("System", "Conversation ended. Agents moved to the selected location.");

        negotiationInProgress = false;
        conversationEnded = true;
        agentsMoved = true;

        StopAllCoroutines();  // Ensure no lingering coroutines
    }

    private void RestartNegotiation()
    {
        if (conversationEnded) return;  // Prevent restarting after conversation ends

        Debug.Log("Agents did not agree. Restarting negotiation...");
        conversationManager.AddConversation("System", "Agents did not agree. Restarting negotiation...");
        negotiationInProgress = false;
        StartLocationNegotiation();
    }

    private string ExtractLocationFromResponse(string response)
    {
        response = response.ToLower().Trim();
        foreach (var location in locations.Keys)
        {
            if (response.Contains(location))
            {
                Debug.Log($"Extracted location: {location}");
                return location;
            }
        }
        Debug.LogWarning($"No valid location found in response: {response}");
        return "";
    }

    private bool IsValidLocation(string locationKey)
    {
        return locations.ContainsKey(locationKey.ToLower().Trim());
    }
}
