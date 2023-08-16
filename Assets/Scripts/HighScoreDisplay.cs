using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreDisplay : MonoBehaviour
{
    public int numberOfTopScoresToShow = 5;
    public Transform entriesParent; // Reference to the parent transform of the Entry objects

    private void Start()
    {
        List<DatabaseManager.DBScore> topScores = DatabaseManager.topScores(numberOfTopScoresToShow);

        for (int i = 0; i < topScores.Count; i++)
        {
            Transform entryTransform = entriesParent.GetChild(i);
            Text usernameText = entryTransform.Find("UsernameText").GetComponent<Text>();
            Text scoreText = entryTransform.Find("ScoreText").GetComponent<Text>();

            if (usernameText != null && scoreText != null)
            {
                usernameText.text = topScores[i].username;
                scoreText.text = $" {topScores[i].score}";
            }
            else
            {
                Debug.LogWarning($"Text components not found in entry {i}");
            }
        }
    }
}
