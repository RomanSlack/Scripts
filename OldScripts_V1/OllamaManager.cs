using UnityEngine;
using System.Diagnostics;
using System.Text;
using System.Collections;
using System;

public class OllamaManager : MonoBehaviour
{
    private const string OLLAMA_PATH = @"C:\Users\roman\AppData\Local\Programs\Ollama\ollama.exe";
    private const string MODEL_NAME = "llama3";

    public IEnumerator AskOllama(string prompt, System.Action<string, GameObject> callback, GameObject agent)
    {
        string arguments = $"run {MODEL_NAME} \"{prompt}\"";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = OLLAMA_PATH,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            StringBuilder output = new StringBuilder();
            process.OutputDataReceived += (sender, e) => { if (!string.IsNullOrEmpty(e.Data)) output.AppendLine(e.Data); };
            process.Start();
            process.BeginOutputReadLine();
            yield return new WaitUntil(() => process.HasExited);

            string result = output.ToString().Trim();
            callback(result, agent); // Call the provided callback
        }
    }
}
