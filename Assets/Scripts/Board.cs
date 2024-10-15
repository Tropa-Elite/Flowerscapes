using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Game.MonoComponent;

public class Board : MonoBehaviour
{
    public GameObject restartButton;
    public TextMeshProUGUI scoreText;
    private int score = 0;

    private void ShowGameOverScreen()
    {
        restartButton.SetActive(true);
    }

    private void IncreaseScore()
    {
        score += 50;
        scoreText.text = $"Score: {score}";
    }
}