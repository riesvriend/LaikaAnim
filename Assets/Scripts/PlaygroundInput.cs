using System.Linq;
using MalbersAnimations.Controller;
using MalbersAnimations.Controller.AI;
using Oculus.Interaction;
using Synchrony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using UnityEngine.Video;
using UnityEngine.Windows;

/// <summary>
/// A or X button = Rotate Animal
/// B or Y button = Rotate video
/// </summary>
public class PlaygroundInput : MonoBehaviour
{
    public RouteHandPoseHandler pettingHandPoseHandler;

    public List<GameObject> animalsToRotate = new List<GameObject>();
    public GameDef activeGame = null;
    public Transform cameraOrEyeTransform;

    public float tableDistanceFromCameraInMeter;
    public float tableHeight = 0.85f;
    public ToggleGroup firstToggleGroup;
    public GameObject animalTogglePrefab;

    /// <summary>
    /// Highlight color/material for ray casting is pointing to the plank
    /// </summary>
    public Material menuPlankHoverMaterial;
    public Material menuPlankNormalMaterial;
    [SerializeField, Interface(typeof(IInteractableView))]
    public MonoBehaviour menuPlankInteractableView;
    public GameObject mainMenu;
    public GameObject artificalGrass;
    public GameObject table;
    public GameObject apple;
    public GameObject comb;

    public AudioSource musicAudioSource;

    public List<SkyboxDescriptor> skyboxDescriptors = new();
    public int activeSkyboxIndex = 0;

    public VideoPlayer skyboxVideoDome;
    public MeshRenderer skyboxPictureDome;

    public bool playBackgroundMusic = true;

    private List<GameDef> gameDefs;

    private Toggle musicToggle;

    // Global keyboard/controller handler
    private GlobalInput globalInput;

    private void Awake()
    {
        try
        {
            globalInput = new GlobalInput();
            globalInput.GlobalControls.Menu.performed += Menu_performed;

            InitAnimalDefs();
            ActivateGameOrAnimal();
            ActivateActiveSkybox();

            InitMainMenu();
            HandleMusicHasChanged(); // Updates player and menu toggle

            InitWoodenPlankMenuButton();
        }
        catch (Exception ex)
        {
            // On Awake regular exception logging in Unity does not yet work 
            // as its hooked up in an Awake event as well which may not yet have run
            // https://stackoverflow.com/questions/15061050/whats-a-good-global-exception-handling-strategy-for-unity3d
            Debug.LogError(ex.Message, this.gameObject);
            Debug.LogError(ex.StackTrace, this.gameObject);
        }
    }

    private void OnEnable()
    {
        globalInput.GlobalControls.Enable();
    }

    private void OnDisable()
    {
        globalInput.GlobalControls.Disable();
    }

    private void Menu_performed(InputAction.CallbackContext obj)
    {
        HandleMenuPlankSelect();
    }

    /// <summary>
    /// Setup meta data for each animal
    /// </summary>
    private void InitAnimalDefs()
    {
        gameDefs = new List<GameDef>();
        var i = 0;

        gameDefs.Add(new GameDef
        {
            gameName = "Home",
            index = i++,
            startGame = PlayHomeScreen
        });

        gameDefs.Add(new GameDef
        {
            gameName = "Puppy Rescue (stationary)",
            index = i++,
            startGame = PlayWithPuppy
        });

        gameDefs.Add(new GameDef
        {
            gameName = "Horse Rescue (room play)",
            index = i++,
            startGame = PlayWithHorse
        });

        foreach (var animalGameObject in animalsToRotate)
        {
            var aiList = animalGameObject.GetComponentsInChildren<MAnimalAIControl>(includeInactive: true);
            var ai = aiList.SingleOrDefault();
            if (ai != null)
                ai.gameObject.SetActive(true); // not all animals have this internal object active out of the box
            var animalDef = new AnimalDef
            {
                gameObject = animalGameObject,
                ai = ai,
                mAnimal = animalGameObject.GetComponent<MAnimal>()
            };
            var gameDef = new GameDef()
            {
                index = i++,
                animal = animalDef,
            };

            if (animalGameObject.name.Equals("Laika"))
            {
                animalDef.animalDistanceFromCameraInMeter = 0.8f;
                animalDef.minComeCloseDistanceFromPlayerInMeter = 1.0f; // no AI so not relevant
            }
            else if (animalGameObject.name.Equals("Paard"))
            {
                animalDef.animalDistanceFromCameraInMeter = 3.5f;
                animalDef.minComeCloseDistanceFromPlayerInMeter = 0.6f;
            }
            else if (animalGameObject.name.Equals("Konijn") || animalGameObject.name.Equals("Puppy"))
            {
                animalDef.animalDistanceFromCameraInMeter = 0.8f;
                animalDef.minComeCloseDistanceFromPlayerInMeter = 0.0f;
                animalDef.mAnimal.CurrentSpeedIndex = 2; // trot
                gameDef.IsTableVisible = true;
            }
            else if (animalGameObject.name.Equals("Olifant"))
            {
                animalDef.animalDistanceFromCameraInMeter = 5.5f;
                animalDef.minComeCloseDistanceFromPlayerInMeter = 2.0f;
            }
            else throw new ApplicationException($"Unknown animal: {animalGameObject.name}");

            gameDefs.Add(gameDef);
        }

        // hack: patchup games with animal refs so games can reference the parameters defined for the animal
        gameDefs.Single(g => g.startGame == PlayWithHorse).animal = gameDefs.Single(g => g.animal?.gameObject.name == "Paard").animal;
        gameDefs.Single(g => g.startGame == PlayWithPuppy).animal = gameDefs.Single(g => g.animal?.gameObject.name == "Puppy").animal;
    }

    private void InitWoodenPlankMenuButton()
    {
        // Clear highlight color
        ClearPlankMenuHighlight();

        var plank = menuPlankInteractableView as IInteractableView;
        plank.WhenStateChanged += Plank_WhenStateChanged;
    }

    private void Plank_WhenStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState == InteractableState.Hover)
        {
            menuPlankRenderer().material = menuPlankHoverMaterial;
        }
        else if (args.NewState == InteractableState.Normal)
        {
            ClearPlankMenuHighlight();
        }
        else if (args.NewState == InteractableState.Select)
        {
            HandleMenuPlankSelect();
        }
    }

    private MeshRenderer menuPlankRenderer()
    {
        return menuPlankInteractableView.GetComponent<MeshRenderer>();
    }

    private void ClearPlankMenuHighlight()
    {
        menuPlankRenderer().material = menuPlankNormalMaterial;
    }

    /// <summary>
    /// Adds toggles to the menu to pick the animals 
    /// </summary>
    private void InitMainMenu()
    {
        // activated by the wooden plank on the side
        ActivatePopupMenu(false);

        // Make the toggles behave like a regular button by allowing it to be toggled off,
        // if it clicked off, we reset to the current animal
        firstToggleGroup.allowSwitchOff = true;
        // The first toggle button is the dummy used for creating for the prefab
        var dummy = firstToggleGroup.GetComponentsInChildren<Toggle>().Single();
        Destroy(dummy.gameObject);

        InitGamesMenu();
        InitSkyboxMenu();
        InitMusicMenu();
    }

    private void PlayWithPuppy()
    {

    }

    private void PlayHomeScreen()
    {
    }

    private void PlayWithHorse()
    {

    }

    private void InitGamesMenu()
    {
        var gamesToggleGroup = firstToggleGroup;

        // Generate toggle buttons from a prefab, one for each item in animalsToRotate
        // Add games and freeplay animals
        foreach (var game in gameDefs)
        {
            var animalToggleGameObject = Instantiate(animalTogglePrefab, parent: firstToggleGroup.transform);
            animalToggleGameObject.SetActive(true); // ...so we have to re-anable it in spawned objects

            var label = animalToggleGameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single();
            label.text = game.name;

            var animalToggle = animalToggleGameObject.GetComponent<Toggle>();
            animalToggle.SetIsOnWithoutNotify(game == activeGame);
            animalToggle.onValueChanged.AddListener((isOn) => GameToggleValueChanged(game, gamesToggleGroup));
        }
    }

    private void InitSkyboxMenu()
    {
        var skyboxToggleGroup = CloneToggleGroup(allowSwitchOff: true);

        var skyboxIndex = 0;
        foreach (var skybox in skyboxDescriptors)
        {
            var skyboxToggleGameObject = Instantiate(animalTogglePrefab, parent: skyboxToggleGroup.transform);
            skyboxToggleGameObject.SetActive(true); // ...so we have to re-anable it in spawned objects

            var label = skyboxToggleGameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single();
            label.text = skybox.name;

            var toggle = skyboxToggleGameObject.GetComponent<Toggle>();
            toggle.SetIsOnWithoutNotify(skyboxIndex == activeSkyboxIndex);
            toggle.onValueChanged.AddListener((isOn) => SkyboxToggleValueChanged(skybox, skyboxToggleGroup));

            skyboxIndex += 1;
        }
    }

    private void InitMusicMenu()
    {
        var musicToggleGroup = CloneToggleGroup(allowSwitchOff: true);

        var toggleGameObject = Instantiate(animalTogglePrefab, parent: musicToggleGroup.transform);
        musicToggle = toggleGameObject.GetComponent<Toggle>();
        toggleGameObject.SetActive(true);

        musicToggle.onValueChanged.AddListener((isOn) => MusicToggleValueChanged(isOn));
    }

    private ToggleGroup CloneToggleGroup(bool allowSwitchOff)
    {
        var animalScollView = firstToggleGroup.transform.parent.parent;
        var musicScrollViewAsObject = GameObject.Instantiate(
            original: animalScollView,
            parent: animalScollView.transform.parent,
            instantiateInWorldSpace: false);

        var clonedScrollViewTransform = musicScrollViewAsObject as UnityEngine.RectTransform;
        var clonedToggleGroupTransform = clonedScrollViewTransform.GetChild(0).GetChild(0);

        // clear the cloned toggles in from the cloned group
        // count will only be adapted in next frame, not during destroy
        var togglesToDelete = clonedToggleGroupTransform.childCount;
        for (int i = 0; i < togglesToDelete; i += 1)
            GameObject.Destroy(clonedToggleGroupTransform.GetChild(i).gameObject);

        var toggleGroup = clonedToggleGroupTransform.GetComponent<ToggleGroup>();
        toggleGroup.allowSwitchOff = allowSwitchOff;

        return toggleGroup;
    }

    void GameToggleValueChanged(GameDef toggledGame, ToggleGroup toggleGroup)
    {
        $"Toggle {toggledGame.name}".Log();

        var toggles = toggleGroup.GetComponentsInChildren<Toggle>();

        foreach (var currentGame in gameDefs)
        {
            var isActiveGame = currentGame == toggledGame;
            var toggle = toggles[currentGame.index];
            toggle.SetIsOnWithoutNotify(isActiveGame);
            if (isActiveGame)
                activeGame = currentGame;
        }

        ActivatePopupMenu(false);
        ActivateGameOrAnimal();
    }

    void SkyboxToggleValueChanged(SkyboxDescriptor toggledSkybox, ToggleGroup toggleGroup)
    {
        $"Toggle {toggledSkybox.name}".Log();

        var toggles = toggleGroup.GetComponentsInChildren<Toggle>();

        var i = 0;
        foreach (var currentSkybox in skyboxDescriptors)
        {
            var isActiveSkybox = currentSkybox == toggledSkybox;
            var toggle = toggles[i];
            toggle.SetIsOnWithoutNotify(isActiveSkybox);
            if (isActiveSkybox)
                activeSkyboxIndex = i;
            i += 1;
        }
        ActivatePopupMenu(false);
        ActivateActiveSkybox();
    }

    void MusicToggleValueChanged(bool isOn)
    {
        playBackgroundMusic = isOn;
        HandleMusicHasChanged();
        ActivatePopupMenu(false);
    }


    private void ActivateActiveSkybox()
    {
        var i = 0;
        foreach (var skybox in skyboxDescriptors)
        {
            var isActive = activeSkyboxIndex == i;
            if (isActive)
                if (skybox.videoClip != null)
                {
                    skyboxVideoDome.clip = skybox.videoClip;
                    ActivateSkyBoxDome(skybox, skyboxVideoDome.gameObject);
                    break;
                }
                else if (skybox.hdriCubeMap != null)
                {
                    skyboxPictureDome.materials[0].SetTexture("_Tex", skybox.hdriCubeMap);
                    ActivateSkyBoxDome(skybox, skyboxPictureDome.gameObject);
                    break;
                }
            i += 1;
        }
    }

    private void ActivateSkyBoxDome(SkyboxDescriptor skybox, GameObject dome)
    {
        var isVideoDome = dome == skyboxVideoDome.gameObject;

        skyboxVideoDome.gameObject.SetActive(isVideoDome);
        skyboxPictureDome.gameObject.SetActive(!isVideoDome);
        dome.transform.rotation = Quaternion.Euler(x: 0, y: skybox.rotation, z: 0);

        // Video's play with an artifical grass floor because due to the low resolution of the video,
        // the earth/ground of the video looks poor on the generated flattened bottom of the sphere
        // For HDRI's, the picture looks very nice as a flat ground plane
        artificalGrass.SetActive(isVideoDome);
    }

    void HandleMusicHasChanged()
    {
        musicAudioSource.enabled = playBackgroundMusic;

        musicToggle.SetIsOnWithoutNotify(playBackgroundMusic);
        var label = musicToggle.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single();
        label.text = playBackgroundMusic ? "Zet Muziek Uit" : "Zet Muziek Aan";
    }


    public void HandleMenuPlankSelect()
    {
        var isActive = !mainMenu.activeSelf;

        ActivatePopupMenu(isActive);
        if (isActive)
            PositionMainMenu();
    }

    private void ActivateGameOrAnimal()
    {
        foreach (var game in gameDefs)
        {
            var isActive = activeGame == game;
            
            var animal = game.animal;

            animal?.gameObject?.SetActive(isActive);

            if (isActive)
            {
                table.SetActive(game.IsTableVisible);

                pettingHandPoseHandler.gameDef = game;

                if (animal?.ai != null)
                {
                    // Prevent it to start running off directly and running through the player
                    animal.ai.Stop();
                    animal.ai.ClearTarget();
                }

                var forward = cameraOrEyeTransform.forward;
                // Forward from camera needs to be projected to the horizontal plane
                forward.y = 0f;
                forward = forward.normalized;
                if (game.IsTableVisible)
                    // When sitting at the table, try to align the virtual table with the real table by disregarding camera/head
                    // and assume the world view is reset to face the table
                    forward = Vector3.forward;

                var animalPos = cameraOrEyeTransform.position;
                if (animal != null) animalPos += forward * animal.animalDistanceFromCameraInMeter;
                Quaternion animalRotation = Quaternion.identity;
                Transform animalTransform = null;
                if (animal != null)
                {
                    // Animal is on the floor (0), unless its on the table
                    animalPos.y = 0f;

                    // https://stackoverflow.com/questions/22696782/placing-an-object-in-front-of-the-camera
                    var animalYRotation = new Quaternion(0.0f, cameraOrEyeTransform.transform.rotation.y, 0.0f, cameraOrEyeTransform.transform.rotation.w).eulerAngles;
                    if (game.IsTableVisible)
                        animalYRotation = forward;

                    animalRotation = Quaternion.Euler(animalYRotation.x + 180, animalYRotation.y, animalYRotation.z + 180);

                    animalTransform = animal.gameObject.transform;
                    animalTransform.position = animalPos;
                    animalTransform.rotation = animalRotation;
                }

                if (game.IsTableVisible)
                {
                    var tablePos = cameraOrEyeTransform.position + forward * tableDistanceFromCameraInMeter;

                    // put the table-top below the camera
                    // TODO: instead of hardcoding the 0.6 meter between eyes and table top, consider
                    // measuring the height of the lowest hand/controller and presume this to be the user's table height
                    // or, give the user a control mechanism to move the virtual table up or down
                    tablePos.y = -0.11f; // 85cm - 11 = 74cm, a common table  // Math.Min(0f, cameraOrEyeTransform.position.y - tableHeight - 0.45f);

                    table.transform.position = tablePos;
                    table.transform.rotation = animalRotation;

                    // Move the animal up, onto the table
                    var animalHeightOnTableTop = Math.Max(0f, tablePos.y + tableHeight + 0.2f /* margin */);
                    if (animalTransform != null)
                        animalTransform.position = new Vector3(animalPos.x, animalHeightOnTableTop, animalPos.z);
                }

                // Place the apple 90 degrees from the animal (to the side of the animal)
                apple.transform.position = animalTransform.position - Quaternion.AngleAxis(140, Vector3.up) * forward * -0.4f;
                comb.transform.position = apple.transform.position + Vector3.up * 0.3f; // Drop the comb on the apple
            }
        }
    }

    private void PositionMainMenu()
    {
        const float radiusOfMenuCylinder = 4f;
        const float distanceFromCameraInMeter = 1.2f - radiusOfMenuCylinder;

        //$"Camera pos: {cameraOrEyeTransform.position}".Log();

        var pos = cameraOrEyeTransform.position + distanceFromCameraInMeter * Vector3.forward; // * cameraOrEyeTransform.forward;
        pos.y = 0.5f; // 1.5f - radiusOfMenuCylinder / 2;
        mainMenu.transform.position = pos;
    }

    private void ActivatePopupMenu(bool activate)
    {
        // only the popup OR the plank can be active, or else the ray casting 
        // gets confused
        // TODO: hide the animals and table that obstruct the menu
        mainMenu.SetActive(activate);
        menuPlankInteractableView.gameObject.SetActive(!activate);
    }

}
