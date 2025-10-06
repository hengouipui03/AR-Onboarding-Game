using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using Vuforia;

public class Station1 : MonoBehaviour
{
    #region Public Variables
    [Header("Others")]
    public CanvasGroup bg;
    public GameObject main;
    public List<int> introSentences;
    public Transform sideBtnsBox;
    public GameObject sideBtns;

    [Header("Quiz")]
    public GameObject quiz;
    public GameObject viewAnswer;
    public GameObject quizBtn;

    [Header("Image & Object Tracking")]
    public GameObject imageTarget;
    public GameObject[] modelTarget;
    public bool imgFound, objectFound;

    [Header("Objective")]
    public GameObject objective;
    public Transform objectiveBox;
    public TextMeshProUGUI objectiveText;
    public string objective1;
    public string objective2;

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
    public List<Sprite> picImg;
    public Transform pBox;

    [Space(10)]
    [Header("Timer")]
    public GameObject returnScreen;

    DialogueManager dialogueManager;
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public VideoPlayer videoPlayer2;
    public GameObject videoScreen;

    #endregion

    int sceneBuildIndex;
    int currPic;

    public void StartVuforiaFocus()
    {
        VuforiaBehaviour.Instance.CameraDevice.SetFocusMode(FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    // Start is called before the first frame update
    void Start()
    {
        //get scene build index and Dialogue Manager
        sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        dialogueManager = FindObjectOfType<DialogueManager>();

        VuforiaApplication.Instance.OnVuforiaStarted += StartVuforiaFocus;

        //pause the timer at the start of the game
        dialogueManager.toPause = true;
        StartCoroutine(dialogueManager.timer());

        //if objects found but havent start quiz
        if (PlayerPrefs.HasKey("Station" + sceneBuildIndex + "ObjectFound"))
        {
            quizBtn.SetActive(true);
        }

        //if still got attempts left or if quiz hasnt started
        if (!PlayerPrefs.HasKey("Question" + FindObjectOfType<Quiz>().questionNumber + "AttemptsLeft") ||
    !(PlayerPrefs.GetInt("Question" + FindObjectOfType<Quiz>().questionNumber + "AttemptsLeft") <= 0))
        {
            StartCoroutine(ShowMapAndImgAnim());
        }
        ShowSideBtns();
    }

    private void Awake()
    {
        //make sure everything is disabled so nothing overlaps
        map.SetActive(false);
        picture.SetActive(false);
        bg.gameObject.SetActive(false);
        returnScreen.SetActive(false);
        videoScreen.SetActive(false);
        objectFound = false;
        currPic = 0;

        for (int i = 0; i < modelTarget.Length; i++)
        {
            modelTarget[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ////if img found, or vid havent play fin and quiz havent fin
        //if ((imgFound || PlayerPrefs.GetFloat("Station" + sceneBuildIndex + "Intro2") == 1) &&
        //    (!PlayerPrefs.HasKey("Station" + sceneBuildIndex + "ObjectFound")))
        //{
        //    //StartCoroutine(ScanTargets());
        //}
        //else
        //{
        //    for (int i = 0; i < modelTarget.Length; i++)
        //    {
        //        modelTarget[i].SetActive(false);
        //    }
        //}
        Debug.Log(videoPlayer2.time + "   " + videoPlayer2.clip.length);
    }

    public IEnumerator ShowMapAndImgAnim()
    {
        //if havent start stn b4
        if (!PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro"))
        {
            StartCoroutine(dialogueManager.StartDialogue());
            yield return new WaitForSeconds(1f);
            yield return new WaitUntil(() => dialogueManager.ended);
            StartCoroutine(FindQRCode());
            PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro", 1);
        }

        else
        {
            //if still got attempts left or if quiz hasnt started
            if (PlayerPrefs.GetFloat("Question" + FindObjectOfType<Quiz>().questionNumber + "AttemptsLeft") != 0 ||
                !PlayerPrefs.HasKey("Question" + FindObjectOfType<Quiz>().questionNumber + "AttemptsLeft"))
            {
                //start timer
                dialogueManager.toPause = true;
                StartCoroutine(dialogueManager.timer());

                int skip = new int();
                //If player hasnt went through the second half
                if (!PlayerPrefs.HasKey("Station" + sceneBuildIndex + "Intro2"))
                {
                    skip = introSentences[0];
                    StartCoroutine(FindQRCode());
                }
                else if (PlayerPrefs.GetFloat("Station" + sceneBuildIndex + "Intro2") == 1)
                {
                    skip = introSentences[1];
                    StartCoroutine(PlayVideo());
                    //play vid here
                }
                else if (PlayerPrefs.GetFloat("Station" + sceneBuildIndex + "Intro2") == 2)
                {
                    skip = introSentences[2];
                    StartCoroutine(ScanTargets());
                }

                for (int i = 0; i < skip; i++)
                {
                    dialogueManager.sentences.Dequeue();
                }

                dialogueManager.audioIndex = skip;
            }
        }

        yield return new WaitUntil(() => dialogueManager.ended);
        OpenMap();
        yield return new WaitUntil(() => !map.activeInHierarchy);
        OpenImg();
        yield return new WaitUntil(() => !picture.activeInHierarchy);
        dialogueManager.toPause = false;
        StartCoroutine(dialogueManager.timer());
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

    public IEnumerator FindQRCode()
    {
        //Wait until picture is closed
        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => !map.activeInHierarchy);
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => !picture.activeInHierarchy);
        imageTarget.SetActive(true);
        //show objective text 1
        objectiveBox.localPosition = new Vector2(0f, -Screen.height);
        objective.SetActive(true);
        objectiveText.text = objective1;
        objectiveBox.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;

        //wait for img to be scanned
        yield return new WaitUntil(() => imgFound);        
        box.LeanMoveLocalY(-Screen.height, 0.5f).setEaseInExpo().delay = 0.1f;
        objective.SetActive(false);

        //introduce the station here
        dialogueManager.ContinueDialogue();
        yield return new WaitUntil(() => dialogueManager.ended);
        PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro2", 1);

        //Play video if station 2, enable model target is station 1
        StartCoroutine(PlayVideo());
        StopCoroutine(FindQRCode());
    }

    public IEnumerator PlayVideo()
    {
        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => !map.activeInHierarchy);
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => !picture.activeInHierarchy);

        yield return new WaitForSeconds(0.5f);
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        dialogueManager.toPause = true;
        dialogueManager.SaveTimer();
        videoScreen.SetActive(true);
        videoPlayer.Play();

        //wait until video ends
        yield return new WaitUntil(() => (int)videoPlayer.time >= (int)videoPlayer.clip.length);
        videoScreen.SetActive(false);
        videoPlayer.gameObject.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;

        yield return new WaitForSeconds(1.5f);        
        dialogueManager.ContinueDialogue();
        yield return new WaitUntil(() => dialogueManager.ended);

        PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "Intro2", 2);
        yield return new WaitForSeconds(1.5f);
        dialogueManager.toPause = false;
        StartCoroutine(dialogueManager.timer());
        StartCoroutine(ScanTargets());

        StopCoroutine(PlayVideo());
    }

    public IEnumerator PlaySecondVideo()
    {
        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => !map.activeInHierarchy);
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => !picture.activeInHierarchy);

        yield return new WaitForSeconds(0.5f);
        dialogueManager.toPause = true;
        dialogueManager.SaveTimer();
        videoScreen.SetActive(true);
        videoPlayer2.Play();

        //wait until video ends
        yield return new WaitUntil(() => videoPlayer2.time+0.25 >= videoPlayer2.clip.length);
        videoScreen.SetActive(false);
        videoPlayer2.gameObject.SetActive(false);

        yield return new WaitForSeconds(1.5f);
        dialogueManager.ContinueDialogue();
        yield return new WaitUntil(() => dialogueManager.ended);

        yield return new WaitForSeconds(1.5f);
        yield return new WaitUntil(() => dialogueManager.startquiz);
        startquizFunc();

        StopCoroutine(PlaySecondVideo());
    }

    public IEnumerator ScanTargets()
    {
        yield return new WaitForSeconds(0.4f);
        yield return new WaitUntil(() => !map.activeInHierarchy);
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => !picture.activeInHierarchy);

        for (int i = 0; i < modelTarget.Length; i++)
        {
            currPic = i + 2;
            OpenImg();
            StartCoroutine(dialogueManager.timer());
            yield return new WaitUntil(() => !picture.activeInHierarchy);
            if (i < modelTarget.Length - 1) //0,1
            {
                modelTarget[i].SetActive(true);
                objectiveBox.localPosition = new Vector2(0f, -Screen.height);
                objective.SetActive(true);
                objectiveText.text = "Find " + modelTarget[i].name + " Object!";
                objectiveBox.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
                yield return new WaitUntil(() => objectFound);
                objective.SetActive(false);
                modelTarget[i].SetActive(false);
                objectFound = false;
                yield return new WaitUntil(() => dialogueManager.ended);
            }
            if (i >= modelTarget.Length - 1) //2
            {
                modelTarget[i].SetActive(true);
                objectiveBox.localPosition = new Vector2(0f, -Screen.height);
                objective.SetActive(true);
                objectiveText.text = "Find " + modelTarget[i].name + "Object!";
                objectiveBox.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
                yield return new WaitUntil(() => objectFound);
                objective.SetActive(false);
                modelTarget[i].SetActive(false);
                objectFound = false;
            }
        }

        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => dialogueManager.ended);
        currPic = 1;
        OpenImg();
        yield return new WaitUntil(() => !picture.activeInHierarchy);
        dialogueManager.ContinueDialogue();
        yield return new WaitUntil(() => dialogueManager.ended);
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(PlaySecondVideo());
        StopCoroutine(ScanTargets());

        //Amen to this code
    }

    public void startquizFunc()
    {
        StartCoroutine(StartQuiz());
    }

    public IEnumerator StartQuiz()
    {
        //closes the main game and opens up the quiz
        yield return new WaitUntil(() => dialogueManager.ended);
        dialogueManager.startquiz = false;
        objective.SetActive(false);
        main.SetActive(false);
        quiz.SetActive(true);
        StartCoroutine(GameObject.Find("GameManager").GetComponent<Quiz>().StartQuizCoroutine());
        dialogueManager.SaveTimer();
        PlayerPrefs.SetFloat("Station" + sceneBuildIndex + "ObjectFound", 1);
        quizBtn.SetActive(true);
        FindObjectOfType<AudioManager>().Play("QuizSound");
    }

    //This is called once the image/Qr code is detected
    public void ChangeBool()
    {
        imgFound = true;
        imageTarget.SetActive(false);
    }

    public void ObjectFound()
    {
        objectFound = true;
    }

    public void ReturnScreen()
    {
        for (int i = 0; i < modelTarget.Length; i++)
        {
            modelTarget[i].SetActive(false);
        }
        imageTarget.SetActive(false);
        bg.gameObject.SetActive(true);
        bg.LeanAlpha(0.6f, 0.5f);
        returnScreen.SetActive(true);
        FindObjectOfType<Quiz>().ResetStation();
    }

    public void ShowSideBtns()
    {
        sideBtns.SetActive(false);
        sideBtnsBox.localPosition = new Vector2(Screen.width, 0);
        sideBtnsBox.LeanMoveX(Screen.width - 70, 0.2f).setEaseOutExpo().delay = 0.1f;
        sideBtns.SetActive(true);
    }

    public void CloseAns()
    {
        viewAnswer.SetActive(false);
        quiz.SetActive(false);
        main.SetActive(true);
        //This is to resume the timer 
        dialogueManager.toPause = false;
        StartCoroutine(dialogueManager.timer());
    }

    //b4 quiz show secret room vid
    //quiz does not pop up (nvm shld work nao)
}
