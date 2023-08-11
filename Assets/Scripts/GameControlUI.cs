using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameControlUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text scoreText;
    [SerializeField]
    private TMP_Text coinText;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score: " + GameControl.Instance.GetScore();
        coinText.text = "Coin: " + GameControl.Instance.GetCoin();
    }
}
