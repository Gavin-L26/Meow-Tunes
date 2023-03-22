using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public static LifeManager current;
    Scene currentScene;
    int buildIndex;

    private void Awake()
    {
        current = this;
    }

    public int playerLives = 5;
    public TMP_Text playerLivesText;
    public GameManager gameManager;
    public AudioSource respawnSound;
    public GameObject Player;
    private PlayerMovement playerMovement;
    public Animator animator;

    private void Start()
    {
        playerMovement = Player.GetComponent<PlayerMovement>();
        animator = Player.GetComponent<Animator>(); 
        currentScene = SceneManager.GetActiveScene();
        buildIndex = currentScene.buildIndex;

        if (buildIndex == 1) // Scene is tutorial
        {
            playerLivesText.text = "x ∞";
        }
        else
        {
            playerLivesText.text = "x " + playerLives;
        }
    }

    public void LostLife()
    {
        playerMovement.SetPlayerInputEnabled(false);
        Debug.Log("player input disabled");
        animator.Play("CatFalling", 0, 0f);
        StartCoroutine(WaitThenEnablePlayerInput());
        respawnSound.Play();
        if (buildIndex != 1) // Scene is tutorial
        {
            playerLives -= 1;
            playerLivesText.text = "x " + playerLives;
            if (playerLives == 0)
            {
                StartCoroutine(LoseLevel());
            }
        }
    }

    private IEnumerator LoseLevel()
    {
        yield return new WaitForSeconds(respawnSound.clip.length - 5f);
        gameManager.LostLevel();
    } 

    private IEnumerator WaitThenEnablePlayerInput()
    {
        yield return new WaitForSeconds(1.5f);
        playerMovement.SetPlayerInputEnabled(true);
        Debug.Log("player input enabled from life manager");
    }
}
