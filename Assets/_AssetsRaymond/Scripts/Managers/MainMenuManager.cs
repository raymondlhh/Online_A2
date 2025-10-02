using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Add this to use SceneManager

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        // BGM is now handled automatically by AudioManager
    }

    // Called when the Start button is pressed
    public void StartButtonPressed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Button Pressed");
        }
        SceneManager.LoadScene("LobbyScene"); // Replace with your exact scene name
    }

    // Called when the Exit button is pressed
    public void ExitButtonPressed()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Button Pressed");
        }
        
        // Quit the application
        #if UNITY_EDITOR
            // If running in the Unity Editor, stop playing
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // If running as a built application, quit the application
            Application.Quit();
        #endif
    }
}
