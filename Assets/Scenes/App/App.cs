using Synchrony;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneEnum
{
    MainMenuScene,
    FloorPlayScene,
    FlyingCarpetScene,
    SampleScene,
}

/// <summary>
/// Global app instance that stays resident across all scenes
/// https://stackoverflow.com/questions/35890932/unity-game-manager-script-works-only-one-time 
/// It is responsible for switching between scenes (games)
/// </summary>
public class App : MonoBehaviour
{
    private SceneEnum? requestedScene = SceneEnum.SampleScene;
    private string currentSceneName = null;

    private void Awake()
    {
        // https://stackoverflow.com/questions/35890932/unity-game-manager-script-works-only-one-time
        // Keep the App loaded when switching scenes
        DontDestroyOnLoad(gameObject);
    }

    public void RequestScene(SceneEnum scene)
    {
        if (currentSceneName != scene.ToString())
        {
            $"RequestScene {scene}".Log();
            requestedScene = scene; // LoadScene(scene.ToString());
        }
    }

    void Update()
    {
        if (requestedScene != null)
        {
            var sceneName = requestedScene.ToString();
            requestedScene = null;
            //StartCoroutine(LoadSceneCoRoutine(requestedScene.Value));
            SceneManager.LoadScene(sceneName);
            currentSceneName = sceneName;
        }
    }

    /* OLD coroutines not yet working */

    public IEnumerator LoadSceneCoRoutine(SceneEnum scene)
    {
        return LoadSceneCoRoutine(scene.ToString());
    }

    public IEnumerator LoadSceneCoRoutine(string sceneName)
    {
        if (currentSceneName != null)
            yield return UnloadSceneCoroutine(currentSceneName); // SceneManager.UnloadScene(currentSceneName);

        // Additive as we keep the app _preload scene containing the
        // shared gameobjects such as audio sources etc
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        currentSceneName = sceneName;
    }

    // https://www.youtube.com/watch?v=3I5d2rUJ0pE (Un)Loading scenes
    private IEnumerable UnloadSceneCoroutine(string sceneName)
    {
        yield return null;
        var operation = SceneManager.UnloadSceneAsync(sceneName);
        while (!operation.isDone)
            yield return null;
    }
}
