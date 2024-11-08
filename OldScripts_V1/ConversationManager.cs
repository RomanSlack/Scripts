using UnityEngine;
using TMPro; // Assuming you're using TextMeshPro
using System.Collections.Generic;

public class ConversationManager : MonoBehaviour
{
    public TMP_Text dialogueText;  // Text object to display the conversation
    public GameObject dialoguePanel;

    private List<string> conversationLog = new List<string>();

    private void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);  // Hide the panel initially
        else Debug.LogError("Dialogue panel not assigned!");

        if (dialogueText == null) Debug.LogError("TMP_Text component not assigned!");
    }

    // Add a message to the conversation and display it
    public void AddConversation(string speaker, string message)
    {
        string formattedMessage = $"{speaker}: {message}";
        conversationLog.Add(formattedMessage);
        UpdateDialogueText();
    }

    // Clear all conversations from the log and hide the panel
    public void ClearConversations()
    {
        conversationLog.Clear();  // Clear the log
        dialogueText.text = "";   // Reset the text
        dialoguePanel.SetActive(false);  // Hide the panel
    }

    // Update the text component with the conversation log
    private void UpdateDialogueText()
    {
        dialoguePanel.SetActive(true);  // Ensure the panel is visible
        dialogueText.text = string.Join("\n", conversationLog);  // Join the log into a single string
    }
}
