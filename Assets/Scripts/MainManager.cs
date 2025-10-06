using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

    //This is to check that the player opened the app already, used in showing the headphones warning
    public bool opened = false;

    //These are the options for the questions, they are set in the inspector
    //Can set the number of options there are for an answer too
    [System.Serializable]
    public class Options
    {
        [TextArea(2, 2)]
        public List<string> option;
    }

    //These are the questions, they can be checked for whether they require the long Mcq buttons or not
    [System.Serializable]
    public class Questions
    {
        [TextArea(5, 5)]
        public string questions;
        public bool longAnswers; //if yes, enable longMcq
    }

    [SerializeField]
    public List<Options> mcqOptions = new List<Options>();
    public List<Questions> mcqQuestions = new List<Questions>();
    [TextArea(5,5)]
    public List<string> mcqAnswers = new List<string>();

    private void Awake()
    {
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;

        StartCoroutine(askForCam());

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator askForCam()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
    }

    //Sets the station's treasure to found
    public void FoundTreasure(int i)
    {
        PlayerPrefs.SetFloat("secretTreasure_" + i, 1);
    }

    public void AddStationQuestions()
    {
        //add bool used and station number
        for (int i = 2; i < 7; i++)
        {
            //Sets all station to "have no question assigned to it"
            PlayerPrefs.SetInt("Station" + i + "HasQuestion", 0);

            //resets the number of attempts for all questions back to 2
            PlayerPrefs.SetInt("Question" + (i - 2) + "AttemptsLeft", 2);
        }
    }
}