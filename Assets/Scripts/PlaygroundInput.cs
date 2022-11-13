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
using UnityEngine.UI;
//using UnityEngine.UIElements;
using UnityEngine.Video;

/// <summary>
/// A or X button = Rotate Animal
/// B or Y button = Rotate video
/// </summary>
public class PlaygroundInput : MonoBehaviour
{
    public RouteHandPoseHandler pettingHandPoseHandler;

    public List<GameObject> animalsToRotate = new List<GameObject>();
    public int activeAnimalIndex = 0;
    public Transform cameraOrEyeTransform;

    public float tableDistanceFromCameraInMeter;
    public float tableHeight = 0.85f;
    public ToggleGroup animalToggleGroup;
    public GameObject animalTogglePrefab;

    /// <summary>
    /// Highlight color/material for ray casting is pointing to the plank
    /// </summary>
    public Material menuPlankHoverMaterial;
    public Material menuPlankNormalMaterial;
    [SerializeField, Interface(typeof(IInteractableView))]
    public MonoBehaviour menuPlankInteractableView;
    public GameObject mainMenu;
    public GameObject table;
    public GameObject apple;
    public GameObject comb;

    public AudioSource musicAudioSource;

    public List<VideoClip> videoClips = new List<VideoClip>();
    public int activeVideoClipIndex = 0;

    public VideoPlayer videoPlayer;

    //enum Sound : int { Silent, VideoOnly, MusicOnly, MusicAndVideo, MaxEnumValue };
    //private Sound currentSoundIndex = Sound.MusicAndVideo;
    public bool playBackgroundMusic = true;
    private int playBackgroundMusicIndex => playBackgroundMusic ? 0 : 1;

    private List<AnimalDef> animalDefs;

    private void Awake()
    {
        InitAnimalDefs();
        ActivateActiveAnimal();
        HandleMusicHasChanged();

        InitMainMenu();
        InitWoodenPlankMenuButton();
    }

    private void Update()
    {
        //OVRInput.Update();

        //// handle the controler or hands "≡" menu button. DOES NOT WORK. BUG in OVR
        //if (OVRInput.GetDown(OVRInput.Button.Start))
        //    HandleMenuPlankSelect();

        //else if (OVRInput.GetDown(OVRInput.Button.Start, OVRInput.Controller.All))
        //    HandleMenuPlankSelect();
    }

    //private void FixedUpdate()
    //{
    //    OVRInput.FixedUpdate();
    //}

    /// <summary>
    /// Setup meta data for each animal
    /// </summary>
    private void InitAnimalDefs()
    {
        this.animalDefs = new List<AnimalDef>();
        var i = 0;
        foreach (var animal in animalsToRotate)
        {
            var aiList = animal.GetComponentsInChildren<MAnimalAIControl>(includeInactive: true);
            var ai = aiList.SingleOrDefault();
            if (ai != null)
                ai.gameObject.SetActive(true); // not all animals have this internal object active out of the box
            var def = new AnimalDef()
            {
                gameObject = animal,
                index = i,
                ai = ai,
                mAnimal = animal.GetComponent<MAnimal>()
            };

            if (animal.name.Equals("Laika"))
            {
                def.animalDistanceFromCameraInMeter = 0.8f;
                def.minComeCloseDistanceFromPlayerInMeter = 1.0f; // no AI so not relevant
            }
            else if (animal.name.Equals("Paard"))
            {
                def.animalDistanceFromCameraInMeter = 3.5f;
                def.minComeCloseDistanceFromPlayerInMeter = 0.6f;
            }
            else if (animal.name.Equals("Konijn") || animal.name.Equals("Puppy"))
            {
                def.animalDistanceFromCameraInMeter = 0.8f;
                def.minComeCloseDistanceFromPlayerInMeter = 0.0f;
                def.IsTableVisible = true;
                def.mAnimal.CurrentSpeedIndex = 2; // trot
            }
            else if (animal.name.Equals("Olifant"))
            {
                def.animalDistanceFromCameraInMeter = 5.5f;
                def.minComeCloseDistanceFromPlayerInMeter = 2.0f;
            }
            else throw new ApplicationException($"Unknown animal: {animal.name}");

            animalDefs.Add(def);
            i += 1;
        }
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

        InitAnimalToggleGroup();

        InitVideoClipMenu();
        InitMusicMenu();
    }

    private void InitVideoClipMenu()
    {
        var clipToggleGroupTransform = CloneToggleGroup();

        var clipIndex = 0;
        foreach (var clip in videoClips)
        {
            var clipToggleGameObject = Instantiate(animalTogglePrefab, parent: clipToggleGroupTransform);
            clipToggleGameObject.SetActive(true); // ...so we have to re-anable it in spawned objects

            var label = clipToggleGameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single();
            label.text = clip.name;

            var clipToggle = clipToggleGameObject.GetComponent<Toggle>();
            clipToggle.SetIsOnWithoutNotify(clipIndex == activeVideoClipIndex);
            clipToggle.onValueChanged.AddListener((isOn) => ClipToggleValueChanged(clipToggle, isOn, clip));

            clipIndex += 1;
        }
    }

    private void InitMusicMenu()
    {
        Transform musicToggleGroupTransform = CloneToggleGroup();

        for (var onOffIndex = 0; onOffIndex < 2; onOffIndex += 1)
        {
            var clipToggleGameObject = Instantiate(animalTogglePrefab, parent: musicToggleGroupTransform);
            clipToggleGameObject.SetActive(true);

            var label = clipToggleGameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single();
            label.text = onOffIndex == 0 ? "Muziek Aan" : "Muziek Uit";

            var clipToggle = clipToggleGameObject.GetComponent<Toggle>();
            clipToggle.SetIsOnWithoutNotify(onOffIndex == playBackgroundMusicIndex);
            var isMusicOn = onOffIndex == 0;
            clipToggle.onValueChanged.AddListener((isOn) => MusicToggleValueChanged(clipToggle, isOn, isMusicOn));
        }
    }

    private Transform CloneToggleGroup()
    {
        var animalScollView = animalToggleGroup.transform.parent.parent;
        var musicScrollViewAsObject = GameObject.Instantiate(
            original: animalScollView,
            parent: animalScollView.transform.parent,
            instantiateInWorldSpace: false);

        var musicScrollViewTransform = musicScrollViewAsObject as UnityEngine.RectTransform;
        var clonedToggleGroupTransform = musicScrollViewTransform.GetChild(0).GetChild(0);

        // clear the cloned toggles in from the cloned group
        // count will only be adapted in next frame, not during destroy
        var togglesToDelete = clonedToggleGroupTransform.childCount;
        for (int i = 0; i < togglesToDelete; i += 1)
            GameObject.Destroy(clonedToggleGroupTransform.GetChild(i).gameObject);
        return clonedToggleGroupTransform;
    }

    private void InitAnimalToggleGroup()
    {
        // The first toggle button is the dummy for the prefab; disable it
        var dummy = animalToggleGroup.GetComponentsInChildren<Toggle>().Single();
        dummy.gameObject.SetActive(false); // somehow this will carry forward to the prefab...

        // Generate toggle buttons from a prefab, one for each item in animalsToRotate
        var animalIndex = 0;
        foreach (var animal in animalsToRotate)
        {
            var animalToggleGameObject = Instantiate(animalTogglePrefab, parent: animalToggleGroup.transform);
            animalToggleGameObject.SetActive(true); // ...so we have to re-anable it in spawned objects

            var label = animalToggleGameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single();
            label.text = animal.name;

            var animalToggle = animalToggleGameObject.GetComponent<Toggle>();
            animalToggle.SetIsOnWithoutNotify(animalIndex == activeAnimalIndex);
            animalToggle.onValueChanged.AddListener((isOn) => AnimalToggleValueChanged(animalToggle, isOn, animal));

            animalIndex += 1;
        }
    }

    void AnimalToggleValueChanged(Toggle toggle, bool isOn, GameObject toggledAnimal)
    {
        $"Toggle {toggledAnimal.name}: {toggle.isOn}".Log();

        if (isOn)
        {
            var i = 0;
            foreach (var currentAnimal in animalsToRotate)
            {
                if (currentAnimal == toggledAnimal)
                {
                    activeAnimalIndex = i;
                    ActivateActiveAnimal();
                    break;
                }
                i += 1;
            }

            ActivatePopupMenu(false);
        }
    }

    void ClipToggleValueChanged(Toggle toggle, bool isOn, VideoClip toggledClip)
    {
        $"Toggle {toggledClip.name}: {toggle.isOn}".Log();

        if (isOn)
        {
            var i = 0;
            foreach (var currentClip in videoClips)
            {
                if (currentClip == toggledClip)
                {
                    activeVideoClipIndex = i;
                    ActivateActiveClip();
                    break;
                }
                i += 1;
            }

            ActivatePopupMenu(false);
        }
    }

    void MusicToggleValueChanged(Toggle toggle, bool isOn, bool isMusicOn)
    {
        playBackgroundMusic = isMusicOn;
        HandleMusicHasChanged();
        ActivatePopupMenu(false);
    }


    private void ActivateActiveClip()
    {
        var i = 0;
        foreach (var clip in videoClips)
        {
            var isActive = activeVideoClipIndex == i;
            if (isActive)
            {
                videoPlayer.clip = clip;
                break;
            }
            i += 1;
        }
    }

    void HandleMusicHasChanged()
    {
        musicAudioSource.enabled = playBackgroundMusic;
    }


    public void HandleMenuPlankSelect()
    {
        var isActive = !mainMenu.activeSelf;

        ActivatePopupMenu(isActive);
        if (isActive)
            PositionMainMenu();
    }

    private void ActivateActiveAnimal()
    {
        foreach (var animal in animalDefs)
        {
            var isActive = activeAnimalIndex == animal.index;
            animal.gameObject.SetActive(isActive);

            if (isActive)
            {
                table.SetActive(animal.IsTableVisible);

                pettingHandPoseHandler.animalDef = animal;

                if (animal.ai != null)
                {
                    // Prevent it to start running off directly and running through the player
                    animal.ai.Stop();
                    animal.ai.ClearTarget();
                }

                var forward = cameraOrEyeTransform.forward;
                // Forward from camera needs to be projected to the horizontal plane
                forward.y = 0f;
                forward = forward.normalized;
                if (animal.IsTableVisible)
                    // When sitting at the table, try to align the virtual table with the real table by disregarding camera/head
                    // and assume the world view is reset to face the table
                    forward = Vector3.forward;

                var animalPos = cameraOrEyeTransform.position + forward * animal.animalDistanceFromCameraInMeter;
                // Animal is on the floor (0), unless its on the table
                animalPos.y = 0f;

                // https://stackoverflow.com/questions/22696782/placing-an-object-in-front-of-the-camera
                var animalYRotation = new Quaternion(0.0f, cameraOrEyeTransform.transform.rotation.y, 0.0f, cameraOrEyeTransform.transform.rotation.w).eulerAngles;
                if (animal.IsTableVisible)
                    animalYRotation = forward;

                var animalRotation = Quaternion.Euler(animalYRotation.x + 180, animalYRotation.y, animalYRotation.z + 180);

                var animalTransform = animal.gameObject.transform;
                animalTransform.position = animalPos;
                animalTransform.rotation = animalRotation;

                if (animal.IsTableVisible)
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
                    animalTransform.position = new Vector3(animalPos.x, animalHeightOnTableTop, animalPos.z);
                }

                // Place the apple 90 degrees from of the animal (to the side of the animal)
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
