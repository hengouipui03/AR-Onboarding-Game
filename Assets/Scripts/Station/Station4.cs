using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class Station4 : MonoBehaviour
{
    #region Public Variables
    [Header("Map & Picture")]
    public GameObject map;
    public Image mapPopUpImg;
    public List<Sprite> mapImg;
    public TextMeshProUGUI mapName;
    public List<string> mapNames;
    public Transform box;
    [Space(5)]
    public GameObject picture;
    public Image picPopUpImg;
    public List<Sprite> picImg;
    public Transform pBox;
    public int currPic;

    [Header("Image & Object Tracking")]
    public List<GameObject> imageTargets;
    public bool imgFound;

    [Header("Objective")]
    public GameObject objective;
    public Transform objectiveBox;
    public TextMeshProUGUI objectiveText;
    public List<string> objectives;

    [Header("Quiz")]
    public GameObject quiz;
    public GameObject viewAnswer;
    public GameObject quizBtn;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public GameObject videoScreen;

    [Header("Others")]
    public CanvasGroup bg;
    public GameObject main;
    public List<int> introSentences;
    public Transform sideBtnsBox;
    public GameObject sideBtns;
    public GameObject returnScreen;

    #endregion

    #region Private Variables
    DialogueManager dialogueManager;
    int sceneBuildIndex;
    #endregion

    public void Start()
    {
        videoScreen.SetActive(false);
        //Change img reference to first location
        currPic = 0;
        //Get build index of current scene
        sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        dialogueManager = FindObjectOfType<DialogueManager>();
        //Pause and start timer to display time left
        dialogueManager.toPause = true;
        StartCoroutine(dialogueManager.timer());

        for (int i = 0; i < imageTargets.Count; i++)
        {
            imageTargets[i].SetActive(false);
        }

        //if model target has been found but quiz has not been touched
        if (PlayerPrefs.HasKey("Station" + sceneBuildIndex + "ObjectFound"))
        {
            //they can start the quiz
            quizBtn.SetActive(true);
        }
        //If player started station for the first time
        if (!PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro"))
        {
            StartCoroutine(dialogueManager.StartDialogue());
            StartCoroutine(switchStations(0));
        }
        else
        {
            //This is the amount of sentences/dialu
            int skip = new int();

            //If player has only finished the first intro
            if(!PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro2"))
            {
                skip = introSentences[0];
                StartCoroutine(switchStations(0));
            }
            //if player has scanned the first qr code
            else if(PlayerPrefs.GetFloat("Station" + sceneBuildIndex + "Intro2") == 1)
            {
                skip = introSentences[1];
                StartCoroutine(switchStations(1));
            }
            //If player has scanned the second qr code
            else if(PlayerPrefs.GetFloat("Station" + sceneBuildIndex + "Intro2") == 2)
            {
                skip = introSentences[2];
                StartCoroutine(switchStations(2));
            }
            //If player has completed the quiz
            if(PlayerPrefs.GetFloat("Question" + FindObjectOfType<Quiz>().questionNumber + "AttemptsLeft") == 0)
            {
                StartCoroutine(switchStations(4));
            }

            //This skips the dialogue based on which part of the station they are left with
            for (int i = 0; i < skip; i++)
            {
                dialogueManager.sentences.Dequeue();
            }
            dialogueManager.audioIndex = skip;
        }
    }

    //This is called once the image/Qr code is detected
    public void ChangeBool(int i)
    {
        imgFound = true;
        imageTargets[i].SetActive(false);
    }

    IEnumerator FindQRCode(int i)
    {
        if (i <= 0 && !PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro"))
        {
            //Wait until the intro dialogue has ended
            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(() => dialogueManager.ended);
        }
        else
        {
            //Wait until picture is closed
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !map.activeInHierarchy);
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !picture.activeInHierarchy);
        }

        //Start the timer and enable the image target
        dialogueManager.toPause = false;
        StartCoroutine(dialogueManager.timer());
        imageTargets[i].SetActive(true);

        //Brings up the objective box and updates the objective
        objectiveBox.localPosition = new Vector2(0f, -Screen.height);
        objective.SetActive(true);
        objectiveText.text = objectives[i];
        objectiveBox.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;

        //wait for img to be scanned
        yield return new WaitUntil(() => imgFound);
        box.LeanMoveLocalY(-Screen.height, 0.5f).setEaseInExpo().delay = 0.1f;
        objective.SetActive(false);

        //introduce the station here
        dialogueManager.ContinueDialogue();
        yield return new WaitUntil(() => dialogueManager.ended);
        //Start the next objective
        StartCoroutine(switchStations(i+1));
        StopCoroutine(FindQRCode(i));
    }

    //double check the qr code orders
    public IEnumerator switchStations(int i)
    {
        currPic = i;
        switch (i)
        {
            //After first QR Code is scanned
            case 0:
                //Start next objective to scan the next qr code
                StartCoroutine(FindQRCode(0));
                yield return new WaitUntil(() => dialogueManager.ended);
                PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro", 1);
                break;

            //After second QR Code is scanned
            case 1:
            case 2:
                imgFound = false;
                //Show map and picture before starting next objective
                OpenMap();
                yield return new WaitUntil(() => !map.activeInHierarchy);
                OpenImg();
                yield return new WaitUntil(() => !picture.activeInHierarchy);
                StartCoroutine(FindQRCode(i));
                yield return new WaitUntil(() => dialogueManager.ended);
                PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro2", i);
                break;

            //Play video & Start the quiz & update the currPic to the last image
            case 3:
                StartCoroutine(PlayVideo());
                yield return new WaitUntil(() => (int)videoPlayer.time >= (int)videoPlayer.clip.length);
                yield return new WaitForSeconds(1.5f);
                dialogueManager.ContinueDialogue();
                yield return new WaitUntil(() => dialogueManager.ended);
                currPic = 2;
                StartQuiz();
                break;

            //Update currpic to last image and do nth
            case 4:
                currPic = 2;
                break;
        }
    }

    public void SwitchMapAndPic(int SwitchIndex)
    {
        if (SwitchIndex == 1)
        {
            OpenMap();
        }
        if (SwitchIndex == 2)
        {
            OpenImg();
        }
        if (SwitchIndex == 3)
        {
            StartCoroutine(CloseMapCoroutine());
        }
        if (SwitchIndex == 4)
        {
            StartCoroutine(ClosePictureCoroutine());
        }
    }

    private void OpenImg()
    {
        picPopUpImg.sprite = picImg[currPic];
        //brings up the pop up reference image
        bg.gameObject.SetActive(true);
        bg.alpha = 0;
        bg.LeanAlpha(0.6f, 0.5f);

        pBox.localPosition = new Vector2(23.25f, -Screen.height);
        //map.SetActive(true);
        pBox.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
        picture.SetActive(true);
        FindObjectOfType<AudioManager>().Play("MapSound");
    }

    private void OpenMap()
    {
        mapPopUpImg.sprite = mapImg[currPic];
        mapName.text = mapNames[currPic];
        //brings up the pop up map and darkens the background
        if (!map.activeInHierarchy)
        {
            bg.gameObject.SetActive(true);
            bg.alpha = 0;
            bg.LeanAlpha(0.6f, 0.5f);

            box.localPosition = new Vector2(23.25f, -Screen.height);
            map.SetActive(true);
            box.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
        }
        FindObjectOfType<AudioManager>().Play("PictureSound");
    }

    public IEnumerator CloseMapCoroutine()
    {
        FindObjectOfType<AudioManager>().Play("CloseSound");
        bg.LeanAlpha(0, 0.3f);
        bg.gameObject.SetActive(false);
        box.LeanMoveLocalY(-Screen.height, 0.3f).setEaseInExpo();
        yield return new WaitForSeconds(0.3f);
        map.SetActive(false);
        StopCoroutine(CloseMapCoroutine());
    }

    public IEnumerator ClosePictureCoroutine()
    {
        FindObjectOfType<AudioManager>().Play("CloseSound");
        bg.gameObject.SetActive(false);
        bg.LeanAlpha(0, 0.3f);
        pBox.LeanMoveLocalY(-Screen.height, 0.3f).setEaseInExpo();
        yield return new WaitForSeconds(0.3f);
        picture.SetActive(false);
    }

    public void StartQuiz()
    {
        //closes the main game and opens up the quiz
        dialogueManager.startquiz = false;
        objective.SetActive(false);
        main.SetActive(false);
        quiz.SetActive(true);
        StartCoroutine(GameObject.Find("GameManager").GetComponent<Quiz>().StartQuizCoroutine());
        dialogueManager.SaveTimer();
        PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "ObjectFound", 1);
        FindObjectOfType<AudioManager>().Play("QuizSound");
    }

    public void CloseAns()
    {
        viewAnswer.SetActive(false);
        quiz.SetActive(false);
        main.SetActive(true);
    }

    public IEnumerator PlayVideo()
    {
        yield return new WaitForSeconds(1f);
        //video shld be in portrait mode since showing the app
        dialogueManager.toPause = true;
        dialogueManager.SaveTimer();
        videoScreen.SetActive(true);
        videoPlayer.Play();

        //wait until video ends
        yield return new WaitUntil(() => (int)videoPlayer.time >= (int)videoPlayer.clip.length);
        videoScreen.SetActive(false);
        videoPlayer.gameObject.SetActive(false);
    }

    //This is called once the timer runs out
    public void ReturnScreen()
    {
        for(int i = 0; i < imageTargets.Count; i++)
        {
            imageTargets[i].SetActive(false);
        }

        bg.gameObject.SetActive(true);
        bg.LeanAlpha(0.6f, 0.5f);
        returnScreen.SetActive(true);
        FindObjectOfType<Quiz>().ResetStation();
    }
}