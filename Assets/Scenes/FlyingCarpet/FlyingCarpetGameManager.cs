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
        app = App.GetApp();

        //app = Synchrony.SynchronyUtils.PreLoadAppScene();
        //if (app == null)
        //    return;
    }

    private void Start()
    {
        // reset the environment scanner, which may still have planes from a previous
        var session = Object.FindObjectsOfType<ARSession>().Single();
        session.Reset();
    }

    /// <summary>
    /// Referenced from FlyingCarpetScene/UI/CarpetUI/Back
    /// </summary>
    public void OnBackHomeClick()
    {
        app.RequestScene(SceneEnum.MainMenuScene);
    }
}
