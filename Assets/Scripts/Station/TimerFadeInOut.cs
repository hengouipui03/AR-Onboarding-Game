using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerFadeInOut : MonoBehaviour
{
    Color color;
    TextMeshProUGUI textMp;
    float timer = 0f;

    private void Start()
    {
        textMp = GetComponent<TextMeshProUGUI>();
        color = textMp.color;
    }

    private void Update()
    {
        if (FindObjectOfType<DialogueManager>().toPause)
        {
            timer += Time.deltaTime;
            if (timer <= 0.55f)
            {
                textMp.color = new Color(color.r/1.1f, color.g/1.1f, color.b/1.1f, 1);
            }
            else if (timer <= 0.9f)
            {
                textMp.color = new Color(color.r/1.1f, color.g/1.1f, color.b/1.1f, 0);
            }
            else
            {
                timer = 0;
            }
        }
        else
        {
            textMp.color = color;
        }

    }
}
