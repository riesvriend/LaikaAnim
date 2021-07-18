using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject planeScanningCanvas;
    [SerializeField] private GameObject carpetPlayCanvas;
    [SerializeField] private GameObject animalToPlacePrefab;

    private StateMachine stateMachine;

    private App app;

    private void Awake()
    {
        app = FindObjectOfType<App>();
        if (app == null)
            Debug.LogError("App not found");

        if (instance != null && instance != this)
            Destroy(gameObject);

        instance = this;
    }

    private void Start()
    {
        stateMachine = new StateMachine();

        //InitGameState();

        //GoToHome();
    }

    /// <summary>
    /// Reset to init-state -- clear properties commonly set to test play inside the editor
    /// </summary>
    private void InitGameState()
    {
        var arPlaneManager = FindObjectsOfType<ARPlaneManager>().Single();
        if (arPlaneManager != null)
            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;

        //mainMenuCanvas.SetActive(false);
        //planeScanningCanvas.SetActive(false);
        //carpetPlayCanvas.SetActive(false);
    }

    private void GoToHome()
    {
        app.RequestScene(SceneEnum.SampleScene); 
        //stateMachine.ChangeAndExecute(new MainMenuState(mainMenuCanvas));
    }

    public void OnFloorPlayClick()
    {
        app.RequestScene(SceneEnum.FloorPlayScene); // new PlaneScanningState(planeScanningCanvas, animalToPlacePrefab));
    }

    public void OnCarpetPlayClick()
    {
        app.RequestScene(SceneEnum.FlyingCarpetScene); // ChangeAndExecute(new CarpetPlayState(carpetPlayCanvas));
    }


    public void OnBackHomeClick()
    {
        GoToHome();
    }
}
