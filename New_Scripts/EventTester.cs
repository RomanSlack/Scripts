using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventTester : MonoBehaviour
{
    public AgentBehavior agentBehavior; // Reference to AgentBehavior
    public int iterations = 20;        // Number of iterations
    public float delayBetweenIterations = 2f; // Delay between each iteration

    private bool isRunning = false; // Flag to prevent re-triggering
    private List<string> unusedMoods = new List<string>(); // Tracks unused moods

    private void Update()
    {
        // Trigger the demo loop on Shift + Space
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.L) && !isRunning)
        {
            StartCoroutine(StartDemo());
        }
    }

    private IEnumerator StartDemo()
    {
        isRunning = true; // Prevent multiple triggers

        // Define moods and reset the list at the start
        string[] moods = { "Want to go home and sleep", "Want to go lift some weights at the gym", "Want to go relax under a tree in the park", "Want to go read some books in the library" };
        unusedMoods = new List<string>(moods); // Reset unused moods

        for (int i = 0; i < iterations; i++)
        {
            // Randomly select a mood from unusedMoods
            string randomMood = GetRandomMood();

            // Set the new mood
            agentBehavior.OnMoodChanged(randomMood);

            // Generate a prompt for the agent
            string prompt = $"Where do you want to move? You are feeling {randomMood}. You can choose between PARK, HOME, GYM, or LIBRARY.";

            // Call AskOllama from AgentBehavior
            yield return agentBehavior.StartCoroutine(agentBehavior.AskOllama(prompt));

            // Wait until the agent reaches the destination
            var navMeshAgent = agentBehavior.moveTool.GetComponent<UnityEngine.AI.NavMeshAgent>();
            while (!HasAgentReachedDestination(navMeshAgent))
            {
                yield return null; // Wait for the agent to stop moving
            }

            Debug.Log($"Iteration {i + 1}/{iterations} completed. Mood: {randomMood}");

            // Delay before the next iteration
            yield return new WaitForSeconds(delayBetweenIterations);

            // Replenish unused moods when they are all used
            if (unusedMoods.Count == 0)
            {
                unusedMoods = new List<string>(moods);
            }
        }

        Debug.Log("Demo completed!");
        isRunning = false; // Allow retriggering
    }

    private string GetRandomMood()
    {
        // Randomly pick a mood from unusedMoods and remove it from the list
        int index = Random.Range(0, unusedMoods.Count);
        string selectedMood = unusedMoods[index];
        unusedMoods.RemoveAt(index);
        return selectedMood;
    }

    private bool HasAgentReachedDestination(UnityEngine.AI.NavMeshAgent navMeshAgent)
    {
        // Check if the agent has reached its destination
        if (!navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < 0.01f)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
