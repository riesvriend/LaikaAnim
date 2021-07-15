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

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        stateMachine = new StateMachine();

        InitGameState();

        GoToHome();
    }

    /// <summary>
    /// Reset to init-state -- clear properties commonly set to test play inside the editor
    /// </summary>
    private void InitGameState()
    {
        var arPlaneManager = FindObjectsOfType<ARPlaneManager>().Single();
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;

        mainMenuCanvas.SetActive(false);
        planeScanningCanvas.SetActive(false);
        carpetPlayCanvas.SetActive(false);
    }

    private void GoToHome()
    {
        stateMachine.ChangeAndExecute(new MainMenuState(mainMenuCanvas));
    }

    /// <summary>
    /// On Floor Play Click
    /// </summary>
    public void OnFloorPlayClick()
    {
        stateMachine.ChangeAndExecute(new PlaneScanningState(planeScanningCanvas, animalToPlacePrefab));
    }

    public void OnCarpetPlayClick()
    {
        stateMachine.ChangeAndExecute(new CarpetPlayState(carpetPlayCanvas));
    }

    public void OnBackHomeClick()
    {
        GoToHome();
    }
}
