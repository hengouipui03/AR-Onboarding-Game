using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Quiz : MonoBehaviour
{   
    [Header("Quiz UI")]
    public GameObject[] btnsShort, btnsLong;
    public GameObject shortMcq, longMcq, questionBox;
    public Transform qBox,sBox,lBox;
    public TextMeshProUGUI question; //to set qn text    

    [Header("CorrectOrWrong")]
    public TextMeshProUGUI correctOrWrong;
    public TextMeshProUGUI attemptsLeft;
    public GameObject correctWrongPopup;    
    public Button retryBtn;
    public Button viewAnsBtn;
    public TextMeshProUGUI viewAnsText;
    [Space(10)]

    [Header("ViewAnswer")]
    public GameObject viewAnswer;
    public TextMeshProUGUI answerText;

    [HideInInspector]
    public int questionNumber = 0;
    string currAns;
    int sceneBuildIndex;
    [Space(5)]
    public GameObject timer;
    public GameObject homeBtn;

    // Start is called before the first frame update
    void Awake()
    {
        //Getting the build index of the current scene
        sceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
        GetQuestion();
        //make sure all the buttons are disabled so they wont overlap each other
        for(int i = 0; i < 4; i++)
        {
            btnsShort[i].SetActive(false);
            btnsLong[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //This checks if the player has started the quiz
        if(PlayerPrefs.HasKey("Question" + questionNumber + "AttemptsLeft"))
        {
            //This checks if the player has completed the quiz
            if (PlayerPrefs.GetInt("Question" + questionNumber + "AttemptsLeft") == 0)
            {
                //Stop and remove timer
                FindObjectOfType<DialogueManager>().toPause = true;
                timer.SetActive(false);
            }
        }
    }

    public void GetQuestion()
    {
        //This checks if the station has a question assigned to it (meaning the player has started the quiz in this station before)
        if (PlayerPrefs.GetInt("Station" + sceneBuildIndex + "HasQuestion") == 1)
        {
            //Find the question number assigned to this station
            //Go through all the questions 
            for (int i = 0; i < MainManager.Instance.mcqQuestions.Count; i++)
            {
                //If the station assigned to question i is this scene
                if (PlayerPrefs.GetInt("StationAssignedtoQuestion" + i) == sceneBuildIndex)
                {
                    questionNumber = i;
                    Debug.Log("Question number is: " + questionNumber);
                }
            }
        }
        else
        {
            ///MAKE SURE THAT THERE ARE ENOUGH QUESTIONS FOR ALL STATIONS OTHERWISE IT WILL CRASH :))
            questionNumber = UnityEngine.Random.Range(0, MainManager.Instance.mcqQuestions.Count);
            //This checks if the question is being used by another station
            while (PlayerPrefs.GetFloat("Question" + questionNumber + "Used") == 1)
            {
                questionNumber = UnityEngine.Random.Range(0, MainManager.Instance.mcqQuestions.Count);
            }

            //Assigns this station to the question
            PlayerPrefs.SetInt("Station" + sceneBuildIndex + "HasQuestion", 1);
            PlayerPrefs.SetFloat("Question" + questionNumber + "Used", 1);
            PlayerPrefs.SetInt("StationAssignedtoQuestion" + questionNumber, sceneBuildIndex);
        }
    }

    public void CheckQuestion()
    {
        //I used the first option in the list as the answer, manually changed/set in the inspector
        currAns = MainManager.Instance.mcqOptions[questionNumber].option[0];
        //Check how many options there are
        int numberOfOptions = MainManager.Instance.mcqOptions[questionNumber].option.Count;
        question.text = MainManager.Instance.mcqQuestions[questionNumber].questions;

        //checks if the questions have long or short answers using the bool longAnswers
        if (MainManager.Instance.mcqQuestions[questionNumber].longAnswers == true)
        {
            SetOptionsLong(numberOfOptions);
        }
        else
        {
            SetOptionsShort(numberOfOptions);
        }
    }

    //This brings up a different button layout for longer mcq answers
    public void SetOptionsLong(int numberOfOptions)
    {
        shortMcq.SetActive(false);
        longMcq.SetActive(true);

        //This list is to hold a temporary random order of the options
        List<int> temp = new List<int>();

        for (int i = 0; i < numberOfOptions; i++)
        {
            int x = UnityEngine.Random.Range(0, numberOfOptions);
            //if random number is already added into list
            while (temp.Contains(x))
            {
                x = UnityEngine.Random.Range(0, numberOfOptions);
            }
            temp.Add(x);
        }

        //setting the texts for the mcq options
        for (int i = 0; i < numberOfOptions; i++)
        {
            btnsLong[i].GetComponentInChildren<TextMeshProUGUI>().text = MainManager.Instance.mcqOptions[questionNumber].option[temp[i]];
            btnsLong[i].SetActive(true);
        }
    }

    //This brings up a different layout for shorter mcq answers
    //works the same way as SetOptionsLong
    public void SetOptionsShort(int numberOfOptions)
    {
        shortMcq.SetActive(true);
        longMcq.SetActive(false);

        List<int> temp = new List<int>();

        for (int i = 0; i < numberOfOptions; i++)
        {
            int x = UnityEngine.Random.Range(0, numberOfOptions);
            while (temp.Contains(x))
            {
                x = UnityEngine.Random.Range(0, numberOfOptions);
            }
            temp.Add(x);
        }

        for (int i = 0; i < numberOfOptions; i++)
        {
            btnsShort[i].GetComponentInChildren<TextMeshProUGUI>().text = MainManager.Instance.mcqOptions[questionNumber].option[temp[i]];
            btnsShort[i].SetActive(true);
        }
    }

    public IEnumerator StartQuizCoroutine()
    {
        homeBtn.SetActive(false);
        FindObjectOfType<DialogueManager>().toPause = true;

        //if no more attempts then view ans
        if (PlayerPrefs.HasKey("Question" + questionNumber + "AttemptsLeft"))
        {
            //Debug.Log("got attempts left key");
            if (PlayerPrefs.GetInt("Question" + questionNumber + "AttemptsLeft") <= 0)
            {
                //Debug.Log("no attempts left, view ans");
                ViewAns();
                yield break;
            }
        }

        else
        {
            PlayerPrefs.SetInt("Question" + questionNumber + "AttemptsLeft", 2);
        }

        //This brings up the question box
        yield return new WaitForSeconds(0.3f);
        qBox.localPosition = new Vector2(0f, Screen.height+200);
        questionBox.SetActive(true);
        qBox.LeanMoveY(Screen.height-60, 0.5f).setEaseOutExpo().delay = 0.1f;

        yield return new WaitForSeconds(0.5f);

        //checks if the questions have long or short answers using the bool longAnswers
        if (MainManager.Instance.mcqQuestions[questionNumber].longAnswers == true)
        {
            //brings up the long answers
            lBox.localPosition = new Vector2(0f, -Screen.height);
            questionBox.SetActive(true);
            lBox.LeanMoveY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
        }
        else
        {
            //brings up the short answers
            sBox.localPosition = new Vector2(0f, -Screen.height);
            questionBox.SetActive(true);
            sBox.LeanMoveY(0, 0.5f).setEaseOutExpo().delay = 0.1f;
        }
        CheckQuestion();
    }

    public void CheckAns()
    {        
        //This popup shows whether the player answered correctly or wrongly
        correctWrongPopup.SetActive(true);
        //If the text of the button is the same as the answer
        //If the answer is correct
        if (EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text == currAns)
        {            
            correctOrWrong.text = "Correct! Well done!";
            attemptsLeft.gameObject.SetActive(false);
            retryBtn.gameObject.SetActive(false);
            viewAnsBtn.gameObject.SetActive(true);
            //Player does not need to redo the quiz anymore
            PlayerPrefs.SetInt("Question" + questionNumber + "AttemptsLeft", 0);
            //Gives player the secret treasure if the quiz has been answered correctly
            MainManager.Instance.FoundTreasure(sceneBuildIndex - 2);
            PlayerPrefs.SetInt("Question" + questionNumber + "CorrectWrong", 1);
            StartCoroutine(CongratulatePlayer());
            //after dis need set up dialogue to congratulate
        }
        else
        {
            correctOrWrong.text = "Wrong answer";
            attemptsLeft.gameObject.SetActive(true);
            attemptsLeft.text = "Attempts Left: " + (PlayerPrefs.GetInt("Question" + questionNumber + "AttemptsLeft")-1);
            retryBtn.gameObject.SetActive(true);
            viewAnsBtn.gameObject.SetActive(false);
            //Reduces attempts left by 1
            int temp = PlayerPrefs.GetInt("Question" + questionNumber + "AttemptsLeft");
            PlayerPrefs.SetInt("Question" + questionNumber + "AttemptsLeft", temp - 1);
        }
        homeBtn.SetActive(true);
    }
    
    public void RetryQuiz()
    {
        //If player has 1 or more retries left
        if(PlayerPrefs.GetInt("Question" + questionNumber + "AttemptsLeft") >= 1)
        {
            //retry;
            correctWrongPopup.SetActive(false);
        }
        else if(PlayerPrefs.GetInt("Question" + questionNumber + "AttemptsLeft") <= 0)
        {
            PlayerPrefs.SetInt("Question" + questionNumber + "CorrectWrong", 0);
            ViewAns();
            StartCoroutine(ConsolePlayer());
        }
    }

    public void ViewAns()
    {
        Debug.Log("Ans viewed");
        qBox.localPosition = new Vector2(0f, Screen.height + 200);
        questionBox.SetActive(true);
        qBox.LeanMoveY(Screen.height - 60, 0.5f).setEaseOutExpo().delay = 0.1f;
        //Gets question to show player
        CheckQuestion();
        correctWrongPopup.SetActive(false);
        answerText.text = MainManager.Instance.mcqAnswers[questionNumber];
        GetQuizStatus();
        viewAnswer.SetActive(true);

        ActivateQuizBtn();
    }

    //This allows the player to open up the quiz by enabling the quiz btn
    void ActivateQuizBtn()
    {
        //Searches for the station script by using the build index
        switch (sceneBuildIndex)
        {
            case 2:
                FindObjectOfType<Station1>().quizBtn.SetActive(true);
                break;
            case 3:
                FindObjectOfType<station2>().quizBtn.SetActive(true);
                break;
            case 4:
                FindObjectOfType<Station3>().quizBtn.SetActive(true);
                break;
            case 5:
                FindObjectOfType<Station4>().quizBtn.SetActive(true);
                break;
        }
    }

    //This changes the text in ViewAns depending on whether the player got the quiz right or wrong
    void GetQuizStatus()
    {
        if (PlayerPrefs.GetInt("Question" + questionNumber + "CorrectWrong") == 0)
        {
            viewAnsText.text = "You did not manage to pass the quiz, and couldn't receive the secret treasure.";
        }
        else if (PlayerPrefs.GetInt("Question" + questionNumber + "CorrectWrong") == 1)
        {
            viewAnsText.text = "Well done! You've completed the trial & obtained the secret treasure!";
        }
    }

    //This resets the entire station, and is called once the timer runs out
    public void ResetStation()
    {
        PlayerPrefs.DeleteKey("Station" + sceneBuildIndex + "Intro");
        PlayerPrefs.DeleteKey("Station" + sceneBuildIndex + "Intro2");
        PlayerPrefs.DeleteKey("Station" + sceneBuildIndex + "Timer");
        PlayerPrefs.DeleteKey("Station" + sceneBuildIndex + "HasQuestion");
        PlayerPrefs.DeleteKey("Station" + sceneBuildIndex + "ObjectFound");
        PlayerPrefs.DeleteKey("StationAssignedToQuestion" + questionNumber);
        PlayerPrefs.DeleteKey("Question" + questionNumber + "Used");
        PlayerPrefs.DeleteKey("Question" + questionNumber + "AttemptsLeft");
        PlayerPrefs.DeleteKey("Question" + questionNumber + "CorrectWrong");
    }

    //This plays when the player managed to answer the quiz correctly
    public IEnumerator CongratulatePlayer()
    {
        yield return new WaitUntil(() => !viewAnswer.activeSelf && !correctWrongPopup.activeSelf);
        FindObjectOfType<DialogueManager>().RightOrWrong();
    }

    //This plays when the player failed to answer the quiz correctly
    public IEnumerator ConsolePlayer()
    {
        yield return new WaitUntil(() => !viewAnswer.activeSelf && !correctWrongPopup.activeSelf);
        FindObjectOfType<DialogueManager>().RightOrWrong();
    }
}
