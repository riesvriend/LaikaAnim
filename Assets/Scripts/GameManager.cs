using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
