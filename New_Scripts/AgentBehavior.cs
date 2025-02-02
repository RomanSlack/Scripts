using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Collections;
using TMPro;

public class AgentBehavior : MonoBehaviour
{
    public string agentName = "Agent_1";  
    public TMP_Text dialogueText; // TextMeshPro for displaying dialogue
    public GameObject dialoguePanel;
    public string mood = "sleepy and want to be somewhere familiar"; 
    public TMP_InputField moodInputField;  
    public string ollamaPath = @"C:\Users\roman\AppData\Local\Programs\Ollama\ollama.exe";
    public string modelName = "qwen:14b";
    
    public Tool_Move moveTool;   
    public Tool_Reset resetTool;
    
    private bool isPrompting = false;

    void Start()
    {
        dialoguePanel.SetActive(true);        
        
        if (moodInputField != null)
        {
            moodInputField.text = mood; 
            moodInputField.onValueChanged.AddListener(OnMoodChanged); 
        }
    }

    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.Space) && !isPrompting)
        {
            isPrompting = true;
            string initialPrompt = $"You are an agent in a multiagent environment. Your task is to choose an action based on your mood. " +
                                   $"The possible actions (tools) are:\n1. MOVE: To move to a location. The location options are PARK, HOME, GYM, and LIBRARY.\n" +
                                   $"2. ERROR: If the response is unclear.\n\n" +
                                   $"You will always output your decision in this format:\n[\"TOOL\", \"CONTEXT\"]\n\n" +
                                   $"Your current mood is {mood}. Where do you want to move?";
            StartCoroutine(AskOllama(initialPrompt));
        }
    }

    public void OnMoodChanged(string newMood)
    {
        mood = newMood; 
    }

    public IEnumerator AskOllama(string prompt)
    {
        string arguments = $"run {modelName} \"{prompt}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ollamaPath,
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
            AppendToDialogue($"LLM Response: {result}"); // Append the raw response
            ProcessResponse(result); // Process the response directly
            isPrompting = false;
        }
    }

    public void AppendToDialogue(string message)
    {
        dialoguePanel.SetActive(true);

        // Append new message to the existing text with a space in between
        dialogueText.text += (string.IsNullOrEmpty(dialogueText.text) ? "" : "\n\n") + message;
    }

    private void ProcessResponse(string response)
    {
        // Parse the response (expected format: ["TOOL", "CONTEXT"])
        response = response.Replace("[", "").Replace("]", "").Replace("\"", "");
        string[] parsedResponse = response.Split(',');

        if (parsedResponse.Length != 2)
        {
            UnityEngine.Debug.LogError("Invalid response format: " + response);
            AppendToDialogue($"Error: Invalid response format."); // Append the error
            resetTool.ExecuteReset("Invalid response format.");
            return;
        }

        string tool = parsedResponse[0].Trim();
        string context = parsedResponse[1].Trim();

        // Log and append the tool and context to the dialogue
        AppendToDialogue($"Tool: {tool}\nContext: {context}");

        // Determine which tool to execute
        switch (tool.ToUpper())
        {
            case "MOVE":
                UnityEngine.Debug.Log($"Agent is moving to: {context}");
                moveTool.ExecuteMove(context);
                break;
            case "ERROR":
                UnityEngine.Debug.LogWarning($"Error occurred: {context}");
                resetTool.ExecuteReset(context);
                break;
            default:
                UnityEngine.Debug.LogError($"Unknown tool: {tool}");
                resetTool.ExecuteReset($"Unknown tool: {tool}");
                break;
        }
    }
}