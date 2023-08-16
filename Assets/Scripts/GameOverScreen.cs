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
    [SerializeField]
    private TMP_Text messageText;
    private float messageDuration = 3f;

    public void Setup(int score)
    {
        gameObject.SetActive(true);
        scoreText.text = "Score: " + score;
        messageText.gameObject.SetActive(false);
    }

    public void SaveScore()
    {
        string inputText = usernameText.text;
        DatabaseManager.AddNewScore(inputText, GameControl.Instance.GetScore(), GameControl.Instance.GetCoin());
        ShowMessage();
    }

    public void ShowMessage()
    {
        messageText.text = "Score Saved !";
        messageText.gameObject.SetActive(true);

        StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }
}
