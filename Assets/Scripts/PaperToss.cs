using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PaperToss : MonoBehaviour
{
    public Transform paperSpawnPoint;
    public GameObject[] paperBalls;
    public float score;
    public TextMeshProUGUI scoreText;
    //public Quaternion phoneAngle, arText;
    public List<Instruction> instructions;
    public GameObject tutorial;
    //public GameObject phoneIndicator;
    bool tutorialOngoing;

    public bool spawned;
    [System.Serializable]
    public class Instruction
    {
        public GameObject instruction;
        //the amount of time the instruction will be on the screen
        public float duration;
    }

    // Start is called before the first frame update
    void Start()
    {
        //sets the screen to potrait mode
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;
        StartCoroutine(SpawnBall());
        StartCoroutine(ShowTutorialCoroutine());
    }

    //this respawns the itme for the player to throw
    public IEnumerator SpawnBall()
    {        
        yield return new WaitForSeconds(1f);
        //gets a random item between cans, rubbish and plastic
        if (!spawned)
        {
            int random = Random.Range(0, paperBalls.Length);
            GameObject paperBall = Instantiate(paperBalls[random], paperSpawnPoint.position, Quaternion.identity);
            paperBall.transform.SetParent(paperSpawnPoint);
            spawned = true;
        }
        StopCoroutine(SpawnBall());
    }

    public void AddScore()
    {
        score += 1;
        FindObjectOfType<AudioManager>().Play("scoreSound");
        DisplayScore();
    }

    public void DecreaseScore()
    {
        score -= 1;
        FindObjectOfType<AudioManager>().Play("loseSound");
        if(score <= 0)
        {
            score = 0;
        }
        DisplayScore();
    }

    public void DisplayScore()
    {
        scoreText.text = score.ToString() + " / 5";

        if(score >= 5)
        {
            SceneManager.LoadScene(4);
        }
    }

    //This starts the tutorial 
    public IEnumerator ShowTutorialCoroutine()
    {
        if (!tutorialOngoing)
        {
            tutorialOngoing = true;
            //brighten indicator
            tutorial.SetActive(true);
            //disables all instructions at the start
            for (int i = 0; i < instructions.Count; i++)
            {
                instructions[i].instruction.SetActive(false);
            }

            yield return new WaitForSeconds(0.5f);

            //shows the instructions in order
            for (int i = 0; i < instructions.Count; i++)
            {
                StartCoroutine(FadeInNOut(instructions[i].instruction.GetComponent<CanvasGroup>(), instructions[i].instruction, instructions[i].duration));
                yield return new WaitForSeconds(instructions[i].duration + 0.5f);
            }
            tutorial.SetActive(false);
            tutorialOngoing = false;
        }
        StopCoroutine(ShowTutorialCoroutine());
    }

    public void ShowTutorial()
    {
        StartCoroutine(ShowTutorialCoroutine());
    }

    //Makes the object show and disappear smoothly
    public IEnumerator FadeInNOut(CanvasGroup cg, GameObject gO, float duration)
    {
        //fade in
        cg.alpha = 0f;
        gO.SetActive(true);
        cg.LeanAlpha(1f, 0.5f);
        //fade out
        yield return new WaitForSeconds(duration);
        cg.LeanAlpha(0f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        gO.SetActive(false);
        StopCoroutine(FadeInNOut(cg, gO, duration));
    }
}
