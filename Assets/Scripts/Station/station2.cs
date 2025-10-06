using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class station2 : MonoBehaviour
{
    #region Public Variables
    [Header("Map & Picture")]
    public GameObject map;
    public Image mapPopUpImg;
    public Sprite mapImg;
    public Transform box;
    public TextMeshProUGUI mapName;
    public string mapNames;
    [Space(5)]
    public GameObject picture;
    public Image picPopUpImg;
    public List<Sprite> picImg;
    public Transform pBox;

    [Header("Image & Object Tracking")]
    public GameObject imageTarget;
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

    [Header("Others")]
    public CanvasGroup bg;
    public GameObject main;
    public List<int> introSentences;
    public Transform sideBtnsBox;
    public GameObject sideBtns;
    public GameObject returnScreen;
    public GameObject guideImg;
    public GameObject homeBtn;

    bool imgShown = false;
    int currPic = 0;

    #endregion

    #region Private Variables
    DialogueManager dialogueManager;
    int sceneBuildIndex;
    #endregion

    public void Start()
    {
        imageTarget.SetActive(false);
        sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        dialogueManager = FindObjectOfType<DialogueManager>();
        dialogueManager.toPause = true;
        StartCoroutine(dialogueManager.timer());
        imageTarget.SetActive(false);

        //if model target has been found but quiz has not been touched
        if (PlayerPrefs.HasKey("Station" + sceneBuildIndex + "ObjectFound"))
        {
            //they can start the quiz
            quizBtn.SetActive(true);
        }
        //If player started station for the first time
        else if (!PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro"))
        {
            //Start the first Intro
            StartCoroutine(dialogueManager.StartDialogue());
            StartCoroutine(OpenMapAndPic());
        }

        else if (PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro"))
        {
            for (int i = 0; i < introSentences[0]; i++)
            {
                dialogueManager.sentences.Dequeue();
            }
            dialogueManager.audioIndex = introSentences[0];
            StartCoroutine(FindQRCode());
        }
    }

    IEnumerator OpenMapAndPic()
    {
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => dialogueManager.ended);
        OpenMap();
        yield return new WaitUntil(() => !map.activeInHierarchy);
        OpenImg();
        yield return new WaitUntil(() => !picture.activeInHierarchy);
        dialogueManager.toPause = false;
        StartCoroutine(dialogueManager.timer());
        StartCoroutine(FindQRCode());
        yield return new WaitUntil(() => dialogueManager.ended);
        PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro", 1);
        StopCoroutine(OpenMapAndPic());
    }

    //This is called once the image/Qr code is detected
    public void QRCodeFound()
    {
        imgFound = true;
        imageTarget.SetActive(false);
    }

    IEnumerator FindQRCode()
    {
        //Wait until picture is closed
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !map.activeInHierarchy);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !picture.activeInHierarchy);
        imageTarget.SetActive(true);
        //show objective text 1

        objectiveBox.localPosition = new Vector2(0f, -Screen.height);
        objective.SetActive(true);
        objectiveText.text = "Find the QR Code located at the Main Entrance!";
        objectiveBox.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;

        //wait for img to be scanned
        yield return new WaitUntil(() => imgFound);
        box.LeanMoveLocalY(-Screen.height, 0.5f).setEaseInExpo().delay = 0.1f;
        objective.SetActive(false);

        //introduce the station here
        dialogueManager.ContinueDialogue();
        StartCoroutine(ShowInstructionPics(1));
        yield return new WaitUntil(() => imgShown);
        dialogueManager.ContinueDialogue();
        StartCoroutine(ShowInstructionPics(2));
        yield return new WaitUntil(() => imgShown);
        dialogueManager.ContinueDialogue();
        StartCoroutine(ShowInstructionPics(3));
        yield return new WaitUntil(() => imgShown);
        StartCoroutine(ShowInstructionPics(4));
        yield return new WaitUntil(() => imgShown);
        StartCoroutine(ShowInstructionPics(5));
        yield return new WaitUntil(() => imgShown);
        dialogueManager.ContinueDialogue();
        StartCoroutine(ShowInstructionPics(6));
        yield return new WaitUntil(() => imgShown);
        dialogueManager.ContinueDialogue();
        StartCoroutine(StartQuiz());
        StopCoroutine(FindQRCode());
    }

    public IEnumerator ShowInstructionPics(int i)
    {
        imgShown = false;
        yield return new WaitUntil(() => dialogueManager.ended);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        homeBtn.SetActive(false);

        guideImg.GetComponentsInChildren<Image>()[1].sprite = picImg[i];
        guideImg.SetActive(true);
        yield return new WaitForSeconds(4f);

        Screen.orientation = ScreenOrientation.Portrait;
        homeBtn.SetActive(true);
        guideImg.SetActive(false);
        imgShown = true;
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
        picPopUpImg.sprite = picImg[0];
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
        mapPopUpImg.sprite = mapImg;
        mapName.text = mapNames;
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

    IEnumerator StartQuiz()
    {
        yield return new WaitUntil(() => dialogueManager.ended);
        yield return new WaitForSeconds(2f);
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

    public void ReturnScreen()
    {
        imageTarget.SetActive(false);
        bg.gameObject.SetActive(true);
        bg.LeanAlpha(0.6f, 0.5f);
        returnScreen.SetActive(true);
        FindObjectOfType<Quiz>().ResetStation();
    }
}