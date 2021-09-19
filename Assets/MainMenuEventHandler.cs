using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuEventHandler : MonoBehaviour
{
    private App app;

    private void Awake()
    {
        app = App.GetApp();
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
