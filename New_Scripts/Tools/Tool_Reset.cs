using UnityEngine;
using TMPro;

public class Tool_Reset : MonoBehaviour
{
    // UI elements for the dialogue box to show the reset message
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;

    // Function to reset the agent's response by displaying an error message
    public void ExecuteReset(string errorReason)
    {
        string errorMessage = "Invalid response: No tool or context detected.";
        ShowDialogue(errorMessage);
    }

    // Function to display the error message on the UI
    public void ShowDialogue(string message)
    {
        dialoguePanel.SetActive(true); // Activate the dialogue panel
        dialogueText.text = message;   // Set the dialogue text to the error message
    }
}
