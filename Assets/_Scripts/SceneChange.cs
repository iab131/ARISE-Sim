using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles transitions between scenes, like main menu to simulation.
/// Attach this to a GameObject in your Main Menu scene (e.g., a "SceneManager" object).
/// </summary>
public class SceneChange: MonoBehaviour
{
    /// <summary>
    /// Loads the simulation scene. Set this to the exact scene name in your Build Settings.
    /// </summary>
    public void LoadSimulationScene()
    {
        SceneManager.LoadScene("Workspace"); // Replace with your actual scene name
    }

    /// <summary>
    /// Quits the application (only works in builds).
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit requested");
        Application.Quit();
    }
}
