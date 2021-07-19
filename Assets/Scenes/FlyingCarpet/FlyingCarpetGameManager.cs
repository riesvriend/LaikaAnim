using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FlyingCarpetGameManager : MonoBehaviour
{
    private App app;

    private void Awake()
    {
        app = FindObjectOfType<App>(); // Lives in the _preloadScene with don't destroy on load
        if (app == null)
            // When playing a scene other than _preLoadScene in the editor
            UnityEngine.SceneManagement.SceneManager.LoadScene("_preloadScene");
    }

    private void GoToHome()
    {
        app.RequestScene(SceneEnum.SampleScene); 
    }

    public void OnBackHomeClick()
    {
        GoToHome();
    }
}
