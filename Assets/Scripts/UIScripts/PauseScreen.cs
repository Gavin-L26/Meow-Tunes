using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseScreen : MonoBehaviour
{
    public GameObject pauseMenuUI, settingsMenuUI;
    public MusicPlayer musicPlayer;
    public GameObject playerMovement;
    private PlayerMovement _playerMovementScript;
    public GameObject pauseScreenFirstButton, settingsFirstButton;

    private void Start()
    {
        _playerMovementScript = playerMovement.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetAxis("Mouse X") != 0 && GameManager.Current.IsGamePaused())
        {
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !GameManager.Current.HasGameEnded() 
            && !CountdownManager.Current.countingDown && !GameManager.Current.IsGamePaused())
        {
            Pause();
        } 
        if (Input.GetKeyDown(KeyCode.Space) && GameManager.Current.IsGamePaused())
        {
            ResumeOrExitSettings();
        } 
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(false);
        CountdownManager.Current.SetCountdown(5f);
        StartCoroutine(ResumeAfterCountdown());
    }

    public IEnumerator ResumeAfterCountdown()
    {
        yield return new WaitForSecondsRealtime(5);
        musicPlayer.Resume();
        Time.timeScale = 1f;
        EnableMovement();
        GameManager.Current.ResumeGame();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        if (CountdownManager.Current.countingDown || GameManager.Current.HasGameEnded() || GameManager.Current.gameIsEnding)
        {
            return;
        }
        musicPlayer.Pause();
        _playerMovementScript.enabled = false;
        PlayerMovement.Current.walkingSound.Stop();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseScreenFirstButton);
        pauseMenuUI.SetActive(true);
        //MidiManager.current.PausePlayback();
        GameManager.Current.PauseGame();
        Time.timeScale = 0f;
        // This is because the camera script locks the cursor,
        // so we need to enable it again to be able to click buttons
        Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        GameManager.Current.RestartLevel();
    }
    public void MainMenu()
    {
        Time.timeScale = 1f;
        GameManager.Current.BackToMainMenu();
    }
    public void PauseController(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (GameManager.Current.HasGameEnded()) return;
            if (!GameManager.Current.IsGamePaused()) Pause();
        }
    }

    public void ResumeController(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (GameManager.Current.HasGameEnded()) return;
            if (GameManager.Current.IsGamePaused()) ResumeOrExitSettings();
        }
    }

    public void OpenSettingsMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsFirstButton);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
    }

    public void EnableMovement()
    {
        _playerMovementScript.enabled = true;
    }

    private void ResumeOrExitSettings()
    {
        if (GameManager.Current.IsGamePaused() && !settingsMenuUI.activeInHierarchy)
        {
            Resume();
        } else if (GameManager.Current.IsGamePaused() && settingsMenuUI.activeInHierarchy)
        {
            SettingsMenu.current.BackToMainMenuOrPauseScreen();
            settingsMenuUI.SetActive(false);
            pauseMenuUI.SetActive(true);
        }
    }

}