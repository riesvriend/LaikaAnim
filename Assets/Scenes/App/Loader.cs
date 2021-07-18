//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public static class Loader
//{
//    private static string currentSceneName = null;

//    public enum SceneEnum
//    {
//        MainMenuScene,
//        FlyingCarpetScene,
//        FloorPlayScene,
//    }

//    public static void Load(SceneEnum scene)
//    {
//        Load(scene.ToString());
//    }

//    public static void Load(string sceneName)
//    {
//        if (currentSceneName != null)
//            SceneManager.UnloadScene(currentSceneName);

//        // Additive as we keep the app _preload scene containing the
//        // shared gameobjects such as audio sources etc
//        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive); 
//        currentSceneName = sceneName;
//    }
//}
