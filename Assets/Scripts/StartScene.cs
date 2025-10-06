using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StartScene : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image progressBarFill;

    //Start the loading scene coroutine
    public void LoadScene(int sceneId)
    {
        FindObjectOfType<AudioManager>().Play("LoadSceneSound");
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    //Loads in the scene based on the id in the build settings
    public IEnumerator LoadSceneAsync(int sceneId)
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        while (!operation.isDone)
        {
            //Simple visual, changing the progress bar's fill amount based on the progress
            float progressValue = Mathf.Clamp01(operation.progress);
            progressBarFill.fillAmount = progressValue;
            yield return null;
        }

    }

    //This is used to reset all the saved data, which are saved using playerprefs
    public void NewUserExperience()
    {
        PlayerPrefs.DeleteAll();
        LoadScene(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}