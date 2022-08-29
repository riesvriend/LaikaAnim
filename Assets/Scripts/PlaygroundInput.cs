using MalbersAnimations.Controller;
using MalbersAnimations.Controller.AI;
using Oculus.Interaction;
using Synchrony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public PettingHandPoseHandler pettingHandPoseHandler;

    public List<GameObject> animalsToRotate = new List<GameObject>();
    public int activeAnimalIndex = 0;
    public Transform cameraOrEyeTransform;

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
            };

            if (animal.name.Equals("Laika"))
            {
                def.initialDistanceFromCameraInMeter = 0.8f;
                def.minComeCloseDistanceFromPlayerInMeter = 1.0f; // no AI so not relevant
            }
            else if (animal.name.Equals("Paard"))
            {
                def.initialDistanceFromCameraInMeter = 3.5f;
                def.minComeCloseDistanceFromPlayerInMeter = 0.4f;
            }
            else if (animal.name.Equals("Konijn") || animal.name.Equals("Puppy"))
            {
                def.initialDistanceFromCameraInMeter = 0.5f;
                def.minComeCloseDistanceFromPlayerInMeter = 0.0f;
            }
            else if (animal.name.Equals("Olifant"))
            {
                def.initialDistanceFromCameraInMeter = 5.5f;
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
        Plank_WhenStateChanged(new InteractableStateChangeArgs { NewState = InteractableState.Normal });

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
        mainMenu.SetActive(false);

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

            mainMenu.SetActive(false);
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

            mainMenu.SetActive(false);
        }
    }

    void MusicToggleValueChanged(Toggle toggle, bool isOn, bool isMusicOn)
    {
        playBackgroundMusic = isMusicOn;
        HandleMusicHasChanged();
        mainMenu.SetActive(false);
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
        mainMenu.SetActive(isActive);
        if (isActive)
            PositionMainMenu();
    }

    private void ActivateActiveAnimal()
    {
        foreach (var animal in animalDefs)
        {
            var isActive = activeAnimalIndex == animal.index;
            animal.gameObject.SetActive(isActive);

            if (isActive && cameraOrEyeTransform != null)
            {
                pettingHandPoseHandler.animalDef = animal;

                if (animal.ai != null)
                {
                    // Prevent it to start running off directly and running through the player
                    animal.ai.ClearTarget();
                }

                var animalPos = cameraOrEyeTransform.position + cameraOrEyeTransform.forward * animal.initialDistanceFromCameraInMeter;
                animalPos.y = 0;
                animal.gameObject.transform.position = animalPos;

                // https://stackoverflow.com/questions/22696782/placing-an-object-in-front-of-the-camera
                var animalRotation = new Quaternion(0.0f, cameraOrEyeTransform.transform.rotation.y, 0.0f, cameraOrEyeTransform.transform.rotation.w).eulerAngles;
                //animalRotation = Quaternion.Inverse(animalRotation);
                animal.gameObject.transform.rotation = Quaternion.Euler(animalRotation.x + 180, animalRotation.y, animalRotation.z + 180);
            }
        }
    }

    private void PositionMainMenu()
    {
        const float radiusOfMenuCylinder = 4f;
        const float distanceFromCameraInMeter = 1.2f - radiusOfMenuCylinder;

        $"Camera pos: {cameraOrEyeTransform.position}".Log();

        var pos = cameraOrEyeTransform.position + distanceFromCameraInMeter * cameraOrEyeTransform.forward;
        pos.y = 0.5f; // 1.5f - radiusOfMenuCylinder / 2;
        mainMenu.transform.position = pos;
    }

}
