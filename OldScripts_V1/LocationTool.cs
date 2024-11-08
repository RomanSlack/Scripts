using UnityEngine;
using System.Collections.Generic;

public class LocationTool : MonoBehaviour
{
    public Dictionary<string, Vector3> locations = new Dictionary<string, Vector3>
    {
        { "location1", new Vector3(-8, 4.8f, 0) },
        { "location2", new Vector3(11, 4.8f, 0) },
        { "location3", new Vector3(-8, -6f, 0) },
        { "location4", new Vector3(11, -6f, 0) }
    };

    public void ExecuteMoveCommand(string locationKey, PlayerPathfinding pathfinding)
    {
        if (locations.TryGetValue(locationKey, out Vector3 newPosition))
        {
            pathfinding.target.position = newPosition;
            pathfinding.StartPathfinding();
        }
        else
        {
            Debug.LogError($"Invalid location: {locationKey}");
        }
    }
}
