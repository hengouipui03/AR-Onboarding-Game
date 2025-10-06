using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("DialogueBox")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public GameObject dialogueBox;
    public Transform box;

    [Header("Timer")]
    public float timeInSeconds;
    public bool toPause = false;

    [Header("Audio")]
    public AudioSource source;
    public List<AudioClip> audioClips;

    [Header("Dialogue")]
    float typingSpeed = 0.045f;
    public dialogue dialogue;
    public Button nextButton;

    [HideInInspector]
    public bool displayNext, pressedNext;
    [HideInInspector]
    public bool startquiz;
    [HideInInspector]
    public bool ended;

    [Space(10)]
    public int animIndex;
    public Animator MascotAnim;
    public Queue<string> sentences;

    public int audioIndex;
    Scene scene;
    bool returnText;

    // Start is called before the first frame update
    void Start()
    {
        ended = true;
        dialogueBox.SetActive(false);
        scene = SceneManager.GetActiveScene();
    }

    private void Update()
    {
    }

    private void Awake()
    {
        sentences = new Queue<string>();
        foreach (string sentence in dialogue.sentences)
        {
            Debug.Log("enqueued");
            sentences.Enqueue(sentence);
        }
    }

    public IEnumerator StartDialogue()
    {
        //Spawn character here
        yield return new WaitForSeconds(0.5f); 

        //bring up the dialogue box
        box.localPosition = new Vector2(0f, -Screen.height);
        dialogueBox.SetActive(true);       
        box.LeanMoveY(0, 0.5f).setEaseOutExpo().delay = 0.1f;

        //sentences.Clear();
        nameText.text = dialogue.name;

        displayNextSentence();
    }

    public void displayNextSentence()
    {
        nextButton.gameObject.SetActive(false);
        ended = false;
        string sentence = sentences.Dequeue();
        StartCoroutine(displayLine(sentence));
    }

    IEnumerator displayLine(string line)
    {
        int letterIndex = 0;
        dialogueText.text = null;
        if (audioIndex <= audioClips.ToArray().Length - 1 && audioClips.ToArray().Length != 0)
        {
            source.clip = audioClips.ToArray()[audioIndex];
            source.Play();
        }
        try
        {
            if (animIndex <= MascotAnim.parameterCount-1)
            {
                MascotAnim.SetTrigger(MascotAnim.parameters[animIndex].name);
                animIndex++;
                Debug.Log(animIndex);
            }
        }
        catch(Exception e)
        {

        }


        foreach (char letter in line.ToCharArray())
        {
            letterIndex++;
            if (letter == '+')
            {
                displayNext = true;
            }
            else if(letter == '-')
            {
                startquiz = true;
            }
            else
            {
                dialogueText.text += letter;
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        if (displayNext)
        {
            yield return new WaitWhile(() => source.isPlaying);
            yield return new WaitForSeconds(0.5f);
            audioIndex++;
            nextButton.gameObject.SetActive(true);
            displayNext = false;
        }
        else
        {
            yield return new WaitWhile(() => source.isPlaying);
            yield return new WaitForSeconds(1.5f);
            EndDialogue();
            audioIndex++;
        }
    }
    
    public void EndDialogue()
    {
        if (!returnText)
        {
            dialogueBox.SetActive(false);
        }
        else
        {
            dialogueBox.GetComponent<Image>().raycastTarget = false;
        }
        toPause = false;
        ended = true;
    }

    bool gotTimer()
    {
        try
        {
            TextMeshProUGUI timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
    }

    public IEnumerator timer()
    {
        Debug.Log("tiemr state");

        if (gotTimer())
        {
            TextMeshProUGUI timerText = GameObject.Find("TimerText").GetComponent<TextMeshProUGUI>();
            if (!PlayerPrefs.HasKey("Station" + scene.buildIndex + "Timer"))
            {
                Debug.Log("stn has no timer");
                PlayerPrefs.SetFloat("Station" + scene.buildIndex + "Timer", 20 * 60f);
            }

            timeInSeconds = PlayerPrefs.GetFloat("Station" + scene.buildIndex + "Timer");
            int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
            int seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);
            timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
            while (timeInSeconds > 1 && !toPause)
            {
                timeInSeconds -= Time.deltaTime;
                minutes = Mathf.FloorToInt(timeInSeconds / 60F);
                seconds = Mathf.FloorToInt(timeInSeconds - minutes * 60);
                timerText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
                yield return null;
            }
            if (timeInSeconds <= 1)
            {
                ReturnScreen(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void ReturnScreen(int i)
    {
        switch (i)
        {
            case 2:
                FindObjectOfType<Station1>().ReturnScreen();
                break;
            case 3:
                FindObjectOfType<station2>().ReturnScreen();
                break;
            case 4:
                FindObjectOfType<Station3>().ReturnScreen();
                break;
            case 5:
                FindObjectOfType<Station4>().ReturnScreen();
                break;
        }
    }

    //Saves the timer where it was left off
    public void SaveTimer()
    {
        if (PlayerPrefs.HasKey("Station" + scene.buildIndex + "Timer"))
        {
            PlayerPrefs.SetFloat("Station" + scene.buildIndex + "Timer", timeInSeconds);
        }
    }

    //saves the timer (Justina, this is garbage data)
    private void OnApplicationQuit()
    {
        SaveTimer();
    }

    public void ContinueDialogue()
    {
        //Debug.Log("hi");
        //pause timer here and save
        nameText.text = dialogue.name;
        toPause = true;
        SaveTimer();
        box.localPosition = new Vector2(0f, -Screen.height);
        dialogueBox.SetActive(true);
        box.LeanMoveY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
        displayNextSentence();
    }

    public void RightOrWrong()
    {
        nameText.text = dialogue.name;
        toPause = true;
        SaveTimer();
        box.localPosition = new Vector2(0f, -Screen.height);
        dialogueBox.SetActive(true);
        box.LeanMoveY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
        returnText = true;
        StartCoroutine(displayLine("Congratulations! You have successfully completed this station, hit the Home Button to return to the Main Menu."));
    }
}
