using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MainMenuGameManager : MonoBehaviour
{
    private App app;

    private void Awake()
    {
        app = App.GetApp(); 
        // FindObjectOfType<App>(); // Lives in the _preloadScene with don't destroy on load
        //if (app == null)
        //    // When playing a scene other than _preLoadScene in the editor
        //    UnityEngine.SceneManagement.SceneManager.LoadScene("_preloadScene");
    }

    public void OnFloorPlayClick()
    {
        app.RequestScene(SceneEnum.FloorPlayScene); // new PlaneScanningState(planeScanningCanvas, animalToPlacePrefab));
    }

    public void OnCarpetPlayClick()
    {
        app.RequestScene(SceneEnum.FlyingCarpetScene); // ChangeAndExecute(new CarpetPlayState(carpetPlayCanvas));
    }
}
