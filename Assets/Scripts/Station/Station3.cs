using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Station3 : MonoBehaviour
{
    #region Public Variables
    [Header("Map & Picture")]
    public GameObject map;
    public UnityEngine.UI.Image mapPopUpImg;
    public Sprite mapImg;
    public Transform box;
    public TextMeshProUGUI mapName;
    public string mapNames;
    [Space(5)]
    public GameObject picture;
    public UnityEngine.UI.Image picPopUpImg;
    public Sprite picImg;
    public Transform pBox;
    public int currPic;

    [Header("Image & Object Tracking")]
    public GameObject imageTargets;
    public bool imgFound;

    [Header("Objective")]
    public GameObject objective;
    public Transform objectiveBox;
    public TextMeshProUGUI objectiveText;

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

    #endregion

    #region Private Variables
    DialogueManager dialogueManager;
    int sceneBuildIndex;

    #endregion

    public void Start()
    {
        currPic = 0;
        sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        dialogueManager = FindObjectOfType<DialogueManager>();
        dialogueManager.toPause = true;
        StartCoroutine(dialogueManager.timer());
        imageTargets.SetActive(false);

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
        
        else if(PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro2"))
        {
            for(int i = 0; i < introSentences[1]; i++)
            {
                dialogueManager.sentences.Dequeue();
            }
            dialogueManager.audioIndex = introSentences[1];
            dialogueManager.ContinueDialogue();
            StartCoroutine(StartQuiz());
        }

        else //When return from game, it comes here
        {
            StartCoroutine(OpenMapAndPic());
            int skip = new int();
            for (int i = 0; i < skip; i++)
            {
                dialogueManager.sentences.Dequeue();
            }
            dialogueManager.audioIndex = skip;
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
        imageTargets.SetActive(false);
    }

    IEnumerator FindQRCode()
    {
        //Wait until picture is closed
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !map.activeInHierarchy);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !picture.activeInHierarchy);
        imageTargets.SetActive(true);
        //show objective text 1

        objectiveBox.localPosition = new Vector2(0f, -Screen.height);
        objective.SetActive(true);
        objectiveText.text = "Find the QR Code!";
        objectiveBox.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;

        //wait for img to be scanned
        yield return new WaitUntil(() => imgFound);
        box.LeanMoveLocalY(-Screen.height, 0.5f).setEaseInExpo().delay = 0.1f;
        objective.SetActive(false);

        //introduce the station here
        //PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro2", 1);
        dialogueManager.ContinueDialogue();
        yield return new WaitUntil(() => dialogueManager.ended); //here say start game
        PlayPaperToss();

        StopCoroutine(FindQRCode());
    }

    //when returned, map n pic is called agn, timer is saved, qr code is up agn
    public void PlayPaperToss()
    {
        PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro2", 1);
        //SceneManager.LoadScene("PaperTossTest");
        FindObjectOfType<StartScene>().LoadScene(6);
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
        picPopUpImg.sprite = picImg;
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
        yield return new WaitForSeconds(1f);
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
        imageTargets.SetActive(false);
        bg.gameObject.SetActive(true);
        bg.LeanAlpha(0.6f, 0.5f);
        returnScreen.SetActive(true);
        FindObjectOfType<Quiz>().ResetStation();
    }

}
