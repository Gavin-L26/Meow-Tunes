using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;


    public class ControlsScreen : MonoBehaviour
    {
        public GameObject ps4Map, keyboardMap;
        private Gamepad _gamepad;
        private PauseScreen _pauseScreenScript;

        private void Start()
        {
            keyboardMap.SetActive(true);
            _gamepad = Gamepad.current;
            _pauseScreenScript = GetComponent<PauseScreen>();
        }

        private void Update()
        {
            if (_gamepad != null)
            {
                keyboardMap.SetActive(false);
                ps4Map.SetActive(true);
            }
            else
            {
                keyboardMap.SetActive(true);
                ps4Map.SetActive(false);
            }
        }
    }
