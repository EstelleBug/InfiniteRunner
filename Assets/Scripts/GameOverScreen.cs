using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField]
    private TMP_Text scoreText;

    public void Setup(int score)
    {
        gameObject.SetActive(true);
        scoreText.text = "Score: " + score;
    }
}
