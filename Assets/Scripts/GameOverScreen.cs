using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private TMP_InputField usernameText;

    public void Setup(int score)
    {
        gameObject.SetActive(true);
        scoreText.text = "Score: " + score;
    }

    public void SaveScore(int score, int coins)
    {
        string inputText = usernameText.text;
        DatabaseManager.AddNewScore(inputText, score, coins);
    }
}
