using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public TMP_Text text;
    public TMP_Text shadow;
    public Image background;
    public float blinkInterval = 1.0f;
    public AudioSource bgMusic;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BlinkText());

        StartCoroutine(SmoothColorTransition(
            background,
            new Color(0, 0, 0, 0),
            200
            ));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene("TUT_00");
        }
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            // Toggle the visibility of the text
            text.enabled = !text.enabled;
            shadow.enabled = !shadow.enabled;

            // Wait for the specified interval
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private IEnumerator SmoothColorTransition(Image targetImage, Color endColor, float transitionDuration)
    {
        float elapsedTime = 0f;        

        while (elapsedTime < transitionDuration)
        {
            targetImage.color = Color.Lerp(targetImage.color, endColor, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetImage.color = endColor; // Ensure the final color is set correctly

    }
}
