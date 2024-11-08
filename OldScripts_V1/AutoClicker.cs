using UnityEngine;

public class AutoClicker : MonoBehaviour
{
    public float clicksPerMinute = 60f;  // Number of clicks per minute
    private float timeBetweenClicks;     // Interval between each click (in seconds)
    private float clickTimer;            // Timer to track the next click

    private void Start()
    {
        // Calculate the interval between clicks based on the clicks per minute
        timeBetweenClicks = 60f / clicksPerMinute;
        clickTimer = timeBetweenClicks;  // Start with the first interval
    }

    private void Update()
    {
        clickTimer -= Time.deltaTime;  // Decrease the timer every frame

        if (clickTimer <= 0f)
        {
            SimulateSpaceKeyPress();  // Trigger the simulated press
            clickTimer = timeBetweenClicks;  // Reset the timer for the next press
        }
    }

    private void SimulateSpaceKeyPress()
    {
        // Find all agents with the LocationSelector component
        LocationSelector[] agents = FindObjectsOfType<LocationSelector>();

        foreach (var agent in agents)
        {
            Debug.Log($"Simulating space key press for {agent.gameObject.name}");
            //agent.TriggerLocationSelection();  // Call the selection logic directly
        }
    }
}
