using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Collections;
using TMPro;

public class Interpreter : MonoBehaviour
{
    // UI elements for the dialogue box to show structured output
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;

    // Path to your local Ollama model (interpreter-specific)
    public string ollamaPath = @"C:\Users\roman\AppData\Local\Programs\Ollama\ollama.exe"; // Adjust this path as necessary
    public string modelName = "llama3";

    // Function to process the agent's response and generate structured output
    // Now passing specific agent's Tool_Move and Tool_Reset scripts directly
    public void ProcessAgentResponse(string agentResponse, string agentName, Tool_Move moveTool, Tool_Reset resetTool)
    {
        // Create the prompt for the interpreter
        string prompt = $"You are an interpreter. Your job is to analyze an agent's response and determine 3 things: which agent it is, what tool they want to use, and the context of the action. " +
                        "You will respond with ONLY a structured output in this format: ['agent_#', TOOL, CONTEXT]. " +
                        "You can identify 2 tools: MOVE (for moving to a location) or ERROR (if the response doesn't make sense). The context for MOVE will be one of these locations: HOME, LIBRARY, GYM, or PARK." +
                        $" The agent's name is {agentName}, and their response was: \"{agentResponse}\".";

        // Call the interpreter (Ollama model)
        StartCoroutine(AskInterpreter(prompt, moveTool, resetTool));
    }

    // Function to request a response from the locally running Ollama model (interpreter)
    public IEnumerator AskInterpreter(string prompt, Tool_Move moveTool, Tool_Reset resetTool)
    {
        // Format the arguments to pass to the local model
        string arguments = $"run {modelName} \"{prompt}\"";

        // Set up the process to execute Ollama (interpreter-specific)
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = ollamaPath,  // The interpreter's specific LLM path
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        // Start the process and read output
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

            // Start the process and begin reading the output
            process.Start();
            process.BeginOutputReadLine();

            // Wait for the process to exit
            yield return new WaitUntil(() => process.HasExited);

            // Get the result from the output
            string result = output.ToString().Trim();

            // Display the structured output from the interpreter in the dialogue box
            ShowStructuredResponse(result);

            // Parse the structured response and determine if an error occurred
            ParseStructuredOutput(result, moveTool, resetTool);
        }
    }

    // Function to display the structured response on the UI
    public void ShowStructuredResponse(string message)
    {
        dialoguePanel.SetActive(true); // Activate the dialogue panel
        dialogueText.text = message;   // Set the dialogue text to the structured response
    }

    // Parse the structured output and determine the tool to use
    private void ParseStructuredOutput(string structuredOutput, Tool_Move moveTool, Tool_Reset resetTool)
    {
        // Example structured output format: ["agent_1", "MOVE", "PARK"] or ["agent_1", "ERROR", "Reason for error"]
        string[] parsedOutput = structuredOutput.Replace("[", "").Replace("]", "").Replace("\"", "").Split(',');

        // Check if the output is correctly structured
        if (parsedOutput.Length != 3)
        {
            UnityEngine.Debug.LogError("Invalid structured output: " + structuredOutput);
            resetTool.ExecuteReset("Invalid structured output.");
            return;
        }

        string agentIdentifier = parsedOutput[0].Trim();
        string tool = parsedOutput[1].Trim();
        string context = parsedOutput[2].Trim();

        // Check which tool to use
        if (tool == "MOVE" || tool == "'MOVE'")
        {
            // Call the Move tool for the specific agent with the destination (e.g., "PARK")
            moveTool.ExecuteMove(context);
        }
        else if (tool == "ERROR")
        {
            // Call the Reset tool for the specific agent with the error reason
            resetTool.ExecuteReset(context);
        }
        else
        {
            // If some unknown tool is detected, call the Reset tool for the agent
            UnityEngine.Debug.LogError("Unknown tool detected: " + tool);
            resetTool.ExecuteReset("Unknown tool detected.");
        }
    }
}
