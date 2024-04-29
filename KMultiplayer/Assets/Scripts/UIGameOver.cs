using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameOver : MonoBehaviour
{

    private Canvas _canvas;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        PlayerController.GameOverEvent += GameOver;

    }
    private void OnDisable()
    {
        PlayerController.GameOverEvent -= GameOver;

    }

    private void GameOver()
    {
        _canvas.enabled = true;
    }
}
