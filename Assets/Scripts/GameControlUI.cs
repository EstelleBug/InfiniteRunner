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
    private TMP_Text bonusText;
    private float messageDuration = 2f;
    [SerializeField]
    private Image distanceParentImage; // Reference to the parent image of the distance text
    private float targetYPosition = -84f;
    private float moveDuration = 0.5f;

    public AudioClip distanceSound;

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
        bonusText.gameObject.SetActive(false);
        distanceParentImage.gameObject.SetActive(false); // Initially hide the distance parent image
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score: " + GameControl.Instance.GetScore();
        coinText.text = "Coin: " + GameControl.Instance.GetCoin();
    }

    public void ShowBonusMessage()
    {
        bonusText.text = "+1000 !";
        bonusText.gameObject.SetActive(true);

        StartCoroutine(HideMessageAfterDelay());
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        bonusText.gameObject.SetActive(false);
    }

    public void ShowDistanceMessage(int distance)
    {
        distanceParentImage.gameObject.SetActive(true);

        Text distanceText = distanceParentImage.GetComponentInChildren<Text>();
        distanceText.text = $" {distance} m ";

        StartCoroutine(AnimateMessage());
        AudioManager.Instance.PlaySound(distanceSound);
    }

    private IEnumerator AnimateMessage()
    {
        RectTransform rectTransform = distanceParentImage.GetComponent<RectTransform>();
        Vector3 initialPosition = rectTransform.anchoredPosition3D;
        Vector3 targetPosition = new Vector3(initialPosition.x, targetYPosition, initialPosition.z);

        // Move message down
        float startTime = Time.time;
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            rectTransform.anchoredPosition3D = Vector3.Lerp(initialPosition, targetPosition, t);
            elapsedTime = Time.time - startTime;
            yield return null;
        }

        yield return new WaitForSeconds(messageDuration);

        // Move message back up
        startTime = Time.time;
        elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            rectTransform.anchoredPosition3D = Vector3.Lerp(targetPosition, initialPosition, t);
            elapsedTime = Time.time - startTime;
            yield return null;
        }

        distanceParentImage.gameObject.SetActive(false); // Hide the distance parent image
    }

}
