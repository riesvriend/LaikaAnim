using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FloorPlayGameManager : MonoBehaviour
{
    [SerializeField] private GameObject planeScanningCanvas;
    [SerializeField] private GameObject animalToPlacePrefab;
    [SerializeField] private GameObject carpetToPlacePrefab;

    private FloorPlayStateMachine stateMachine;
    private App app;

    private void Awake()
    {
        app = App.GetApp(); 

        stateMachine = new FloorPlayStateMachine();
    }

    private void Start()
    {
        stateMachine.ChangeAndExecute(new PlaneScanningState(planeScanningCanvas, animalToPlacePrefab, carpetToPlacePrefab));
    }

    private void GoToHome()
    {
        app.RequestScene(SceneEnum.MainMenuScene); 
    }

    public void OnBackHomeClick()
    {
        GoToHome();
    }
}
