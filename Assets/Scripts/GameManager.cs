using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject planeScanningCanvas;
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
        stateMachine.ChangeAndExecute(new MainMenuState(mainMenuCanvas));
    }

    public void StartPlaneScanning()
    {
        stateMachine.ChangeAndExecute(new PlaneScanningState(planeScanningCanvas, animalToPlacePrefab));
    }
}
