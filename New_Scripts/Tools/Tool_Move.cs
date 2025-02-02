using UnityEngine;
using UnityEngine.AI;

public class Tool_Move : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    public float proximityRadius = 5f; // Radius to detect nearby agents
    public LayerMask agentLayer; // Layer for detecting agents

    private AgentBehavior parentAgent; // Reference to the parent AgentBehavior script

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        parentAgent = GetComponent<AgentBehavior>();
    }

    private void Start()
    {
        animator.SetBool("isWalking", false);
        agent.baseOffset = 0.1f;
    }

    private void Update()
    {
        if (IsAtDestination())
        {
            StopAgent();
        }
        else if (agent.velocity.sqrMagnitude > 0.1f)
        {
            StartWalking();
        }
        else
        {
            StopWalking();
        }
    }

    public void ExecuteMove(string destination)
    {
        Vector3 targetPosition = ConvertDestinationToCoordinates(destination);

        if (targetPosition != Vector3.zero)
        {
            Debug.Log($"Moving to: {targetPosition}");
            agent.SetDestination(targetPosition);
        }
        else
        {
            Debug.LogWarning("Invalid destination provided.");
        }
    }

    private Vector3 ConvertDestinationToCoordinates(string destination)
    {
        // Random offset within a 5-unit range
        float offsetX = Random.Range(-5f, 5f);
        float offsetZ = Random.Range(-5f, 5f);

        switch (destination.ToUpper())
        {
            case "PARK":
                return new Vector3(215.586f + offsetX, 0.175f, 168.429f + offsetZ);
            case "HOME":
                return new Vector3(192.77f + offsetX, 0.175f, 178.854f + offsetZ);
            case "GYM":
                return new Vector3(215.88f + offsetX, 0.175f, 196.194f + offsetZ);
            case "LIBRARY":
                return new Vector3(221.387f + offsetX, 0.175f, 178.17f + offsetZ);
            default:
                Debug.LogWarning($"Unknown destination: {destination}");
                return Vector3.zero;
        }
    }

    private bool IsAtDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void StopAgent()
    {
        agent.ResetPath();
        animator.SetBool("isWalking", false);
        //Debug.Log("Agent has reached the destination.");

        // Stop agent rotation
        agent.updateRotation = false;

        
    }

    private void StartWalking()
    {
        animator.SetBool("isWalking", true);

        // Allow agent rotation during walking
        agent.updateRotation = true;

        Vector3 direction = agent.velocity.normalized;
        if (direction.sqrMagnitude > 0.1f) // Prevent jittering due to very small adjustments
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void StopWalking()
    {
        animator.SetBool("isWalking", false);
    }

    
}
