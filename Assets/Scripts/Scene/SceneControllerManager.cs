using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerManager : SingletonMonobehaviour<SceneControllerManager>
{
   private bool isFading;
   [SerializeField]private float fadeDuration = 1f;
   [SerializeField]private CanvasGroup faderCanvasGroup = null;
   [SerializeField]private Image faderImage = null;
    public SceneName startingSceneName;

    public void FadeAndLoadScene(SceneName sceneName,Vector3 spawnPosition)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndSwitchScene(sceneName, spawnPosition));
        }
    }

    private IEnumerator Fade(float targetAlpha)
    {
        isFading = true;
        faderCanvasGroup.blocksRaycasts = true;
        float speed = Mathf.Abs(faderCanvasGroup.alpha - targetAlpha) / fadeDuration;
        while (!Mathf.Approximately(faderCanvasGroup.alpha, targetAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
        isFading = false;
        faderCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeAndSwitchScene(SceneName sceneName, Vector3 spawnPosition)
    {
       EventHandler.CallBeforeSceneUnloadFadeOutEvent();
       yield return StartCoroutine(Fade(1f));
       Player.Instance.gameObject.transform.position = spawnPosition;
       EventHandler.CallBeforeSceneUnloadEvent();
       yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
       yield return StartCoroutine(LoadSceneAndSetActive(sceneName));
       EventHandler.CallAfterSceneLoadFadeInEvent();
       yield return StartCoroutine(Fade(0f));
       EventHandler.CallAfterSceneLoadEvent();
    }

    private IEnumerator LoadSceneAndSetActive(SceneName sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);
        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newScene);
    }

    private IEnumerator Start()
    {
        faderImage.color = new Color(0f, 0f, 0f, 1f);
        faderCanvasGroup.alpha = 1f;
        yield return StartCoroutine(LoadSceneAndSetActive(startingSceneName));
        EventHandler.CallAfterSceneLoadEvent();
        yield return StartCoroutine(Fade(0f));
    }
}
