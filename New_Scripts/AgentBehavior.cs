using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Collections;
using TMPro;

public class AgentBehavior : MonoBehaviour
{
    // Identifier for the agent
    public string agentName = "Agent_1";  // Set unique name for each agent

    // UI elements for the dialogue box
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;

    // Mood variable that can be customized via Input Field
    public string mood = "sleepy and want to be somewhere familiar"; // Default mood

    // Reference to the Input Field where the user can change the mood
    public TMP_InputField moodInputField;  // Link this in the Unity Inspector

    // Path to your local Ollama model
    public string ollamaPath = @"C:\Users\roman\AppData\Local\Programs\Ollama\ollama.exe"; // Adjust for each agent in Unity Inspector
    public string modelName = "llama3";  // Each agent can use the same or different model

    // Reference to the Interpreter and Tools
    private Interpreter interpreter;
    public Tool_Move moveTool;   // Set the correct Tool_Move for this agent in Unity
    public Tool_Reset resetTool; // Set the correct Tool_Reset for this agent in Unity

    // Track whether Ollama has been queried
    private bool isPrompting = false;

    void Start()
    {
        dialoguePanel.SetActive(true);        
        interpreter = FindObjectOfType<Interpreter>(); // Find the interpreter in the scene
        
        // If the Input Field exists, set the default mood and listen for changes
        if (moodInputField != null)
        {
            moodInputField.text = mood; // Set the default value in the Input Field
            moodInputField.onValueChanged.AddListener(OnMoodChanged); // Listen for input changes
        }
    }

    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Space) && !isPrompting)
        {
            isPrompting = true; // Prevent multiple triggers
            string initialPrompt = $"Where do you want to move? You exist as an agent in a simplistic multiagent environment. You have 4 options (park, home, gym, library). Please briefly respond with which one you want to choose, and then shortly explain why. Let's assume right now your mood is {mood}.";
            StartCoroutine(AskOllama(initialPrompt));
        }
    }

    // Called when the mood input field value changes
    public void OnMoodChanged(string newMood)
    {
        mood = newMood;  // Update the mood in the script
    }

    public IEnumerator AskOllama(string prompt)
    {
        string arguments = $"run {modelName} \"{prompt}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ollamaPath,  // Use the public path set in the Unity Inspector
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            StringBuilder output = new StringBuilder();
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    output.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            yield return new WaitUntil(() => process.HasExited);

            string result = output.ToString().Trim();
            ShowDialogue(result);

            // Send agent-specific LLM details to the interpreter along with the agent's tools
            interpreter.ProcessAgentResponse(result, agentName, moveTool, resetTool);
            isPrompting = false;
        }
    }

    public void ShowDialogue(string message)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = message;
    }

      public string GetCurrentResponse()
    {
        return dialogueText.text;
    }
}
