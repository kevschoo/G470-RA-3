using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class UIPlayerStatus : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lengthText;

    void Start()
    {
        
    }

    void OnEnable()
    {
        PlayerLength.ChangedLengthEvent += ChangeScore;
    }

    void OnDisable()
    {
        PlayerLength.ChangedLengthEvent -= ChangeScore;
    }

    private void ChangeScore(int score)
    {
        string scoreText = score.ToString();
        lengthText.text = scoreText;
    }
}
