using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    bool gameHasEnded = false;
    public GameObject completeLevelUI;

    private void Update() {
        if (Input.GetButton("Start") && gameHasEnded) {
            restartLevel();
        }
    }
    public void wonLevel() {
        if (gameHasEnded == false) {
            gameHasEnded = true;
            completeLevelUI.SetActive(true);
        }
    }

    void restartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gameHasEnded = false;
    }
}
