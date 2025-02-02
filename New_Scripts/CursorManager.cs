using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public GameObject moodInputField; // Reference to the mood input field GameObject
    public MonoBehaviour playerController; // Reference to the first-person controller script (e.g., a movement script)

    private bool isCursorUnlocked = false;

    private void Start()
    {
        LockCursor(); // Start with cursor locked for first-person control
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // Press Tab to toggle cursor state
        {
            isCursorUnlocked = !isCursorUnlocked;

            if (isCursorUnlocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Enable the mood input field for interaction
        if (moodInputField != null)
            moodInputField.SetActive(true);

        // Disable player input (but don't stop scene updates)
        if (playerController != null)
            playerController.enabled = false; // Disable input control script
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Disable the mood input field
        if (moodInputField != null)
            moodInputField.SetActive(false);

        // Enable player input
        if (playerController != null)
            playerController.enabled = true;
    }
}
