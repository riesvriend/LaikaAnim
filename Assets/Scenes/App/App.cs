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
    //SampleScene,
}

/// <summary>
/// Global app instance that stays resident across all scenes
/// https://stackoverflow.com/questions/35890932/unity-game-manager-script-works-only-one-time 
/// It is responsible for switching between scenes (games)
/// </summary>
public class App : MonoBehaviour
{
    private SceneEnum? requestedScene = SceneEnum.MainMenuScene;
    private string currentSceneName = null;

    private void Awake()
    {
        // https://stackoverflow.com/questions/35890932/unity-game-manager-script-works-only-one-time
        // Keep the App loaded when switching scenes. We use this as a simple alternative
        // over using UnloadSceneAsync and LoadSceneMode.Additive
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
            StartCoroutine(LoadSceneCoRoutine(sceneName));
            // TODO: Consider making a App-game object visible that shows a spinning loading indicator
            // until LoadSceneCoRoutine is done
            currentSceneName = sceneName;
        }
    }

    public IEnumerator LoadSceneCoRoutine(string sceneName)
    {
        // https://www.youtube.com/watch?v=3I5d2rUJ0pE (Un)Loading scenes
        var operation = SceneManager.LoadSceneAsync(sceneName); //, LoadSceneMode.Additive)
        while (!operation.isDone)
            yield return null;
    }
}
