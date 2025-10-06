using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Warning")]
    public GameObject warning;
    bool warningClosed;

    [Header("Intro")]
    public GameObject mainMenu;
    public GameObject background;
    public GameObject dBox;
    public GameObject character;    
    public Transform box;
    public GameObject compass;
    public GameObject groundPlane;

    [Header("Treasure")]
    public TextMeshProUGUI treasuresFound;
    public GameObject[] treasureIcons;
    
    [Header("Tutorial")]
    public GameObject tutorial;
    public List<CanvasGroup> hintBoxesCg = new List<CanvasGroup>();
    public List<GameObject> hintBoxes = new List<GameObject>();
    public List<GameObject> fillImages = new List<GameObject>();

    [Header("CompletionScreen")]
    public GameObject completionScreen;
    public Transform completionScreenBox;
    public GameObject trophy;

    //this font got no numbers so nid use words
    List<string> numbers = new List<string> { "Zero", "One", "Two", "Three", "Four", "Five" };

    //public GameObject cam1, cam2;

    public Canvas canvas;

    DialogueManager dialogueManager;

    // Start is called before the first frame update
    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();
        compass.SetActive(false);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainMenu.SetActive(true);
        background.SetActive(true);
        //SetCams(cam1, cam2);
        tutorial.SetActive(false);
        groundPlane.SetActive(false);

        //Check if user has re-opened app or not
        if (!MainManager.Instance.opened)
        {
            //Open up the warning to wear headsets
            warning.SetActive(true);
            warningClosed = false;
            MainManager.Instance.opened = true;
        }

        StartCoroutine(CheckCompletion());

        //This checks if the player hasnt gone through the app introduction
        if (!PlayerPrefs.HasKey("intro"))
        {
            //the main menu will only be disabled for the intro, afterwards it'll remain open
            StartCoroutine(StartIntro());
        }

        if (MainManager.Instance.opened && (PlayerPrefs.GetFloat("intro") == 1))
        {
            StartCoroutine(ShowTreasuresFound());
        }
    }

    public IEnumerator CheckCompletion()
    {
        yield return new WaitUntil(() => !warning.activeInHierarchy);
        bool completed = true;
        for (int i = 0; i < 4; i++)
        {
            if (!PlayerPrefs.HasKey("Station" + (i + 2) + "ObjectFound"))
            {
                completed = false;
            }
        }
        if (completed)
        {
            trophy.SetActive(true);
        }

        if (completed && !PlayerPrefs.HasKey("CompleteScreenShown"))
        {
            completionScreen.SetActive(false);
            completionScreenBox.localPosition = new Vector2(0f, Screen.height);
            completionScreen.SetActive(true);
            completionScreenBox.LeanMoveLocalY(0, 1.5f).setEaseOutBounce().delay = 0.1f;
            PlayerPrefs.SetFloat("CompleteScreenShown", 1);
        }
        else
        {
            completionScreen.SetActive(false);
        }
    }

    public void CloseCompletionScreen()
    {
        trophy.SetActive(true);
        completionScreenBox.LeanMoveLocalY(-Screen.height, 0.5f).setEaseInExpo().delay = 0.1f;
        completionScreen.SetActive(false);
    }

    //wait until character spawn then start dialogue
    public IEnumerator StartIntro()
    {
        compass.SetActive(true);
        mainMenu.SetActive(false);
        background.SetActive(false);
        yield return new WaitUntil(() => warningClosed);

        StartCoroutine(ShowTutorial(0, 4));
        yield return new WaitUntil(() => !tutorial.activeInHierarchy);
        groundPlane.SetActive(true);
        yield return new WaitUntil(() => FindObjectOfType<testplace>().spawned);
        StartCoroutine(dialogueManager.StartDialogue());

        //open main menu canvas
        yield return new WaitForSeconds(2.5f);
        //wait until the dialogue box isn't active, character isnt needed anymore, slide main menu back up
        yield return new WaitUntil(() => !dBox.activeInHierarchy);

        groundPlane.SetActive(false); 
        box.localPosition = new Vector2(0f, -Screen.height);
        background.SetActive(true);
        mainMenu.SetActive(true);        
        box.LeanMoveLocalY(0, 0.5f).setEaseOutExpo().delay = 0.1f;

        compass.SetActive(false);
        StartCoroutine(ShowTutorial(5,9));
        //Save data that player has gone through intro
        PlayerPrefs.SetFloat("intro", 1);
        StartCoroutine(ShowTreasuresFound());
    }

    //This coroutine shows the treasures found indicator one by one according to the stations. 
    public IEnumerator ShowTreasuresFound()
    {
        yield return new WaitUntil(() => !warning.activeInHierarchy);
        yield return new WaitUntil(() => !completionScreen.activeInHierarchy);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        //SetCams(cam1, cam2);

        if (!MainManager.Instance.opened)
        {
            //If so, then wait until the warning is closed
            yield return new WaitUntil(() => warningClosed);
        }

        yield return new WaitForSeconds(1f);
        //This list will hold the number of treasures found and from which station it was found
        List<int> temp = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (PlayerPrefs.HasKey("secretTreasure_" + i))
            {
                temp.Add(i);
            }
        }
        treasuresFound.text = "Special Treasures Found: " + numbers[temp.Count];
        //This enables the treasure icons based on the temp list
        for (int x = 0; x < temp.Count; x++)
        {
            Debug.Log("treasure" + x + " found");
            treasureIcons[temp[x]].GetComponent<Image>().enabled = true;
            treasureIcons[temp[x]].GetComponent<Image>().GetComponentInChildren<ParticleSystem>().Play();
            FindObjectOfType<AudioManager>().Play("CoinsSound");
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void CloseWarning()
    {
        if (PlayerPrefs.GetFloat("intro") == 1)
        {
            ShowTreasuresFound();
        }
        warning.SetActive(false);
        warningClosed = true;
    }

    public IEnumerator ShowTutorial(int start, int amt)
    {
        tutorial.SetActive(true);
        for (int i = start; i < amt+1; i++)
        {
            hintBoxes[i].SetActive(false);
        }

        yield return new WaitForSeconds(1f);
        for (int i = start; i < amt+1; i++)
        {
            StartCoroutine(FadeInNOut(hintBoxesCg[i], hintBoxes[i]));
            yield return new WaitForSeconds(7f);
        }
        tutorial.SetActive(false);
    }

    public IEnumerator FadeInNOut(CanvasGroup cg, GameObject gO)
    {
        //fade in
        cg.alpha = 0f;
        gO.SetActive(true);
        cg.LeanAlpha(1f, 0.5f);
        //fade out
        yield return new WaitForSeconds(6f);
        cg.LeanAlpha(0f, 0.5f);
        yield return new WaitForSeconds(1f);
        gO.SetActive(false);
    }

    public void SetCams(GameObject cam1,GameObject cam2)
    {
        cam1.SetActive(true);
        cam2.SetActive(false);
    }
}

//main menu tut not higlighting correctly.
//quiz home btn can be pressed.