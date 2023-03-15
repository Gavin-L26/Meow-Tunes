using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Current;

    public GameObject backMenuKeyboard, backMenuPS4, backMenuXbox;
    public GameObject winLevelUI;
    public PlayerInput playerInput;
    public GameObject winLevelReplayPS4, winLevelReplayKeyboard, winLevelReplayXbox;
    public GameObject loseLevelReplayPS4, loseLevelReplayKeyboard, loseLevelReplayXbox;
    public TMP_Text winLevelFishScoreText;
    public TMP_Text winLevelAccuracyScoreText;
    public TMP_Text winLevelFinalScoreText;
    public GameObject lostLevelUI;
    public GameObject gameUI;
    public GameObject loadingScreen;

    // tutorial sign buttons
    public static GameObject PS4Jump, XboxJump, KeyboardJump;
    public static GameObject PS4Eat, XboxEat, KeyboardEat;
    public static GameObject PS4Stomp, XboxStomp, KeyboardStomp;
    public static GameObject PS4Jump2, XboxJump2, KeyboardJump2;
    public GameObject dpad, arrows;

    public GameObject[] ps4Tutorial = new GameObject[]{ PS4Jump , PS4Eat , PS4Stomp , PS4Jump2 };
    public GameObject[] xboxTutorial = new GameObject[] { XboxJump, XboxEat, XboxStomp, XboxJump2 };
    public GameObject[] keyboardTutorial = new GameObject[] { KeyboardJump, KeyboardEat, KeyboardStomp, KeyboardJump2 };
    private bool _tutorialScene;

    private void Awake()
    {
        Current = this;
        CheckIfPlayerUsingControllerOrKeyboard();
    }

    private void Start()
    {
        _tutorialScene = SceneManager.GetActiveScene().buildIndex == 1;
    }

    private void CheckIfPlayerUsingControllerOrKeyboard()
    {
        SetReplayActiveBasedOnInputMethod(CheckLastUpdatedInputMethod());
        InputUser.onChange += (_, change, _) =>
        {
            if (change is InputUserChange.ControlSchemeChanged)
            {
                SetReplayActiveBasedOnInputMethod(CheckLastUpdatedInputMethod());
            }
        };
    }

    private string CheckLastUpdatedInputMethod()
    {
        if (playerInput.currentControlScheme.ToLower().Contains("gamepad") ||
            playerInput.currentControlScheme.ToLower().Contains("joystick"))
        {
            return "gamepad";
        }

        return "keyboard";
    }

    private void SetReplayActiveBasedOnInputMethod(string lastUpdatedInputMethod)
    {
        if (lastUpdatedInputMethod == "gamepad")
        {
            var gamepad = Gamepad.current;
            if (gamepad is XInputController || gamepad is SwitchProControllerHID)
            {
                // back from menu
                backMenuKeyboard.SetActive(false);
                backMenuPS4.SetActive(false);
                backMenuXbox.SetActive(true);
                // win
                winLevelReplayKeyboard.SetActive(false);
                winLevelReplayPS4.SetActive(false);
                winLevelReplayXbox.SetActive(true);
                // lose
                loseLevelReplayKeyboard.SetActive(false);
                loseLevelReplayPS4.SetActive(false);
                loseLevelReplayXbox.SetActive(true);
                if (_tutorialScene)
                {
                    // Tutorial sign xbox
                    for (int i = 0; i < 4; i++)
                    {
                        ps4Tutorial[i].SetActive(false);
                        xboxTutorial[i].SetActive(true);
                        keyboardTutorial[i].SetActive(false);
                    }
                    dpad.SetActive(true);
                    arrows.SetActive(false);
                }
            }
            else
            {
                // back from menu
                backMenuKeyboard.SetActive(false);
                backMenuPS4.SetActive(true);
                backMenuXbox.SetActive(false);
                // win
                winLevelReplayKeyboard.SetActive(false);
                winLevelReplayPS4.SetActive(true);
                winLevelReplayXbox.SetActive(false);
                // lose
                loseLevelReplayKeyboard.SetActive(false);
                loseLevelReplayPS4.SetActive(true);
                loseLevelReplayXbox.SetActive(false);
                if (_tutorialScene)
                {
                    // Tutorial sign PS4
                    for (int i = 0; i < 4; i++)
                    {
                        ps4Tutorial[i].SetActive(true);
                        xboxTutorial[i].SetActive(false);
                        keyboardTutorial[i].SetActive(false);
                    }
                    dpad.SetActive(true);
                    arrows.SetActive(false);
                }
            }
        }
        else
        {
            // back from menu
            backMenuKeyboard.SetActive(true);
            backMenuPS4.SetActive(false);
            backMenuXbox.SetActive(false);
            // win
            winLevelReplayKeyboard.SetActive(true);
            winLevelReplayPS4.SetActive(false);
            winLevelReplayXbox.SetActive(false);
            // lose
            loseLevelReplayKeyboard.SetActive(true);
            loseLevelReplayPS4.SetActive(false);
            loseLevelReplayXbox.SetActive(false);
            if (_tutorialScene)
            {
                // Tutorial sign keyboard
                for (int i = 0; i < 4; i++)
                {
                    ps4Tutorial[i].SetActive(false);
                    xboxTutorial[i].SetActive(false);
                    keyboardTutorial[i].SetActive(true);
                }
                dpad.SetActive(false);
                arrows.SetActive(true);
            }
        }
    }

    public void WonLevelUI()
    {
        // Text Example: Collected 87/100 Fish Treats!
        winLevelFishScoreText.text = ScoreManager.current.playerFishScore.ToString() + "/" + ScoreManager.current.maximumFishScore.ToString();
                                 
        // Text Example: Meowsic Score: 1,050
        winLevelAccuracyScoreText.text = ScoreManager.current.playerAccuracyScore.ToString("n0");

        // Text Example: Final Score: 87 x 1050 = 91,350
        winLevelFinalScoreText.text = "Final Score: " + (ScoreManager.current.playerFishScore*ScoreManager.current.playerAccuracyScore).ToString("n0");

        gameUI.SetActive(false);
        winLevelUI.SetActive(true);
        SetReplayActiveBasedOnInputMethod(CheckLastUpdatedInputMethod());
    }

    public void LostLevelUI()
    {
        gameUI.SetActive(false);
        lostLevelUI.SetActive(true);

    }

    public void DisplayLoadingScreen()
    {
        loadingScreen.SetActive(true);
    }
}
