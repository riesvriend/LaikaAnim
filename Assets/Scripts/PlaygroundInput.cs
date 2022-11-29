using Oculus.Interaction;
using Synchrony;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// A or X button = Rotate Animal
/// B or Y button = Rotate video
/// </summary>
public class PlaygroundInput : MonoBehaviour
{
    public RouteHandPoseHandler pettingHandPoseHandler;

    public List<GameObject> animalsToRotate = new List<GameObject>();
    public GameInstance activeGame = null;
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

    // Animal defs are used as prefabs, one for each type of animal. The template animals
    // are kept in-active and are instantiated by each game
    public List<AnimalDef> animalDefs = new List<AnimalDef>();

    private List<GameDef> gameDefs = new List<GameDef>();

    private Toggle musicToggle;

    // Global keyboard/controller handler
    private GlobalInput globalInput;

    private void Awake()
    {
        try
        {
            globalInput = new GlobalInput();
            globalInput.GlobalControls.Menu.performed += Menu_performed;
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

    private void Start()
    {
        InitAnimalDefs();
        InitGameDefs();
        ActivateActiveSkybox();

        // Start the game before init of main menu, which hightlights the current game's toggle
        StartHomeScreenGame();

        InitMainMenu();
        HandleMusicHasChanged(); // Updates player and menu toggle
        InitWoodenPlankMenuButton();
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
        AddAnimalDef(
            AnimalDef.Snowdog,
            "Laika",
            new AnimalDef
            {
                animalDistanceFromCameraInMeter = 0.8f,
                minComeCloseDistanceFromPlayerInMeter = 1.0f,
                CanBeBrushed = false,
                EatsApples = false,
            }
        );
        AddAnimalDef(
            AnimalDef.Horse,
            "Paard",
            new AnimalDef
            {
                animalDistanceFromCameraInMeter = 3.5f,
                minComeCloseDistanceFromPlayerInMeter = 0.5f,
                CanBeBrushed = true,
                EatsApples = true,
            }
        );
        AddAnimalDef(
            AnimalDef.Rabbit,
            "Konijn",
            new AnimalDef
            {
                animalDistanceFromCameraInMeter = 0.8f,
                minComeCloseDistanceFromPlayerInMeter = 0.0f,
                CanBeBrushed = true,
                EatsApples = true,
            },
            speedIndex: 2
        );
        AddAnimalDef(
            AnimalDef.WolfPuppy,
            "Puppy",
            new AnimalDef
            {
                animalDistanceFromCameraInMeter = 0.8f,
                minComeCloseDistanceFromPlayerInMeter = 0.0f,
                CanBeBrushed = true,
                EatsApples = false,
            },
            speedIndex: 2
        );
        AddAnimalDef(
            AnimalDef.Elephant,
            "Olifant",
            new AnimalDef
            {
                animalDistanceFromCameraInMeter = 5.5f,
                minComeCloseDistanceFromPlayerInMeter = 2.0f,
                CanBeBrushed = true,
                EatsApples = true,
            }
        );
    }

    private void InitGameDefs()
    {
        AddGameDef(
            new GameDef { name = "Home", GameType = typeof(HomeScreenGame) },
            animals: new string[] { }
        );

        AddGameDef(
            new GameDef
            {
                name = "Rabbit Rescue (stationary)",
                GameType = typeof(StationaryGame),
                IsTableVisible = true,
                IsCombVisible = true,
                IsAppleVisible = true,
            },
            animals: new[] { AnimalDef.Rabbit, AnimalDef.Rabbit }
        );

        AddGameDef(
            new GameDef
            {
                name = "Horse Rescue (room play)",
                GameType = typeof(RoomScaleGame),
                IsAppleVisible = true,
                IsCombVisible = true,
            },
            animals: new[] { AnimalDef.Horse }
        );

        AddGameDef(
            new GameDef { name = "Aai Laika", GameType = typeof(JustShowTheAnimalGame) },
            animals: new[] { AnimalDef.Snowdog }
        );

        AddGameDef(
            new GameDef
            {
                name = "Paard",
                GameType = typeof(JustShowTheAnimalGame),
                IsAppleVisible = true,
                IsCombVisible = true,
            },
            animals: new[] { AnimalDef.Horse }
        );

        AddGameDef(
            new GameDef
            {
                name = "Konijn",
                GameType = typeof(JustShowTheAnimalGame),
                IsTableVisible = true,
                IsAppleVisible = true,
                IsCombVisible = true,
            },
            animals: new[] { AnimalDef.Rabbit }
        );

        AddGameDef(
            new GameDef
            {
                name = "Wolf Puppy",
                GameType = typeof(JustShowTheAnimalGame),
                IsTableVisible = true,
                IsCombVisible = true,
            },
            animals: new[] { AnimalDef.WolfPuppy }
        );

        AddGameDef(
            new GameDef
            {
                name = "Olifant",
                GameType = typeof(JustShowTheAnimalGame),
                IsAppleVisible = true,
            },
            animals: new[] { AnimalDef.Elephant }
        );
    }

    private GameDef AddGameDef(GameDef gameDef, string[] animals)
    {
        // Assign an animal instance to the game
        foreach (var animalDefName in animals)
        {
            var animalDef = animalDefs.Single(a => a.name == animalDefName);
            gameDef.animals.Add(animalDef);
        }
        gameDefs.Add(gameDef);
        return gameDef;
    }

    private void AddAnimalDef(
        string name,
        string templateGameObjectName,
        AnimalDef animalDef,
        int speedIndex = 2
    )
    {
        animalDef.name = name;
        animalDef.templateGameObject = animalsToRotate.Single(
            a => a.gameObject.name == templateGameObjectName
        );
        animalDef.templateGameObject.SetActive(false);
        if (animalDef.mAnimal != null)
            animalDef.mAnimal.CurrentSpeedIndex = speedIndex;
        animalDefs.Add(animalDef);
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

    private void Play(GameDef gameDef)
    {
        if (activeGame != null)
        {
            Destroy(activeGame);
            activeGame = null;
        }

        activeGame = (GameInstance)gameObject.AddComponent(gameDef.GameType);
        activeGame.gameDef = gameDef;
        activeGame.playground = this;
        activeGame.StartGame();
    }

    private void StartHomeScreenGame()
    {
        Play(gameDefs[0]); //.First(g => g.name.Contains("Puppy")));
    }

    private void InitGamesMenu()
    {
        var gamesToggleGroup = firstToggleGroup;

        // Generate toggle buttons from a prefab, one for each item in animalsToRotate
        // Add games and freeplay animals
        foreach (var game in gameDefs)
        {
            var gameToggleGameObject = Instantiate(
                animalTogglePrefab,
                parent: firstToggleGroup.transform
            );
            gameToggleGameObject.SetActive(true); // ...so we have to re-anable it in spawned objects

            var label = gameToggleGameObject
                .GetComponentsInChildren<TMPro.TextMeshProUGUI>()
                .Single();
            label.text = game.name;

            var gameToggle = gameToggleGameObject.GetComponent<Toggle>();
            gameToggle.SetIsOnWithoutNotify(game == activeGame.gameDef);
            gameToggle.onValueChanged.AddListener(
                (isOn) => GameToggleValueChanged(game, gameToggle, gamesToggleGroup)
            );
        }
    }

    private void InitSkyboxMenu()
    {
        var skyboxToggleGroup = CloneToggleGroup(allowSwitchOff: true);

        var skyboxIndex = 0;
        foreach (var skybox in skyboxDescriptors)
        {
            var skyboxToggleGameObject = Instantiate(
                animalTogglePrefab,
                parent: skyboxToggleGroup.transform
            );
            skyboxToggleGameObject.SetActive(true); // ...so we have to re-anable it in spawned objects

            var label = skyboxToggleGameObject
                .GetComponentsInChildren<TMPro.TextMeshProUGUI>()
                .Single();
            label.text = skybox.name;

            var toggle = skyboxToggleGameObject.GetComponent<Toggle>();
            toggle.SetIsOnWithoutNotify(skyboxIndex == activeSkyboxIndex);
            toggle.onValueChanged.AddListener(
                (isOn) => SkyboxToggleValueChanged(skybox, skyboxToggleGroup)
            );

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
            instantiateInWorldSpace: false
        );

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

    void GameToggleValueChanged(GameDef toggledGame, Toggle toggle, ToggleGroup toggleGroup)
    {
        // A toggle goes from on to off and back, but we want to use it as a button
        // here so we just force it to on as the clicked game's toggle is active
        var toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        foreach (var t in toggles)
            t.SetIsOnWithoutNotify(t == toggle);

        ActivatePopupMenu(false);

        Play(toggledGame);
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
