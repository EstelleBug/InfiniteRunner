using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameControlUI : MonoBehaviour
{
    public static GameControlUI Instance;

    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private TMP_Text coinText;
    [SerializeField]
    private TMP_Text messageText;
    private float messageDuration = 2f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        messageText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score: " + GameControl.Instance.GetScore();
        coinText.text = "Coin: " + GameControl.Instance.GetCoin();
    }

    public void ShowMessage()
    {
        messageText.text = "+1000 !";
        messageText.gameObject.SetActive(true);

        StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
    }
}
