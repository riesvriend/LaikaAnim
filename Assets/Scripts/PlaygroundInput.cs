using MalbersAnimations.Controller.AI;
using Oculus.Interaction;
using Synchrony;
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

    private void Awake()
    {
        ActivateActiveAnimal();
        ActivateSound();

        InitMainMenu();
        InitWoodenPlankMenuButton();
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
    }

    private void InitVideoClipMenu()
    {
        var animalScollView = animalToggleGroup.transform.parent.parent;
        var clipScrollViewAsObject = GameObject.Instantiate(
            original: animalScollView,
            parent: animalScollView.transform.parent,
            instantiateInWorldSpace: false);

        var clipScrollViewTransform = clipScrollViewAsObject as UnityEngine.RectTransform;
        var clipToggleGroupTransform = clipScrollViewTransform.GetChild(0).GetChild(0);

        // clear the cloned toggles in from the cloned group
        // count will only be adapted in next frame, not during destroy
        var togglesToDelete = clipToggleGroupTransform.childCount;
        for (int i = 0; i < togglesToDelete; i += 1)
            GameObject.Destroy(clipToggleGroupTransform.GetChild(i).gameObject);

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

    void Update()
    {
        // music (B: button on contrller or M on keyboard)
        if (OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.M))
        {
            playBackgroundMusic = !playBackgroundMusic;

            ActivateSound();
        }
    }

    void ActivateSound()
    {
        musicAudioSource.enabled = playBackgroundMusic;

        //musicAudioSource.enabled = (currentSoundIndex == Sound.MusicOnly || currentSoundIndex == Sound.MusicAndVideo);
        //if (currentSoundIndex == Sound.MusicAndVideo || currentSoundIndex == Sound.VideoOnly)
        //    videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        //else
        //    videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
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
        var i = 0;
        foreach (var animal in animalsToRotate)
        {
            var isActive = activeAnimalIndex == i;
            
            var ai = animal.GetComponentsInChildren<MAnimalAIControl>(includeInactive: true);
            if (ai.Length == 1)
            {
                // Prevent it from away from its start position and running through the player
                ai[0].ClearTarget();
                "Clearing AI Target".Log();
            }

            animal.SetActive(isActive);
            if (isActive && cameraOrEyeTransform != null)
            {
                float distanceFromCameraInMeter = 0.5f;
                if (animal.name.Equals("Laika")) // i = 0
                    distanceFromCameraInMeter = 0.7f;
                else if (i == 1)
                    // hack: horse is larger
                    distanceFromCameraInMeter = 3.5f;
                else if (i == 4)
                    // hack: elephant is larger
                    distanceFromCameraInMeter = 3.5f;

                var animalPos = cameraOrEyeTransform.position + cameraOrEyeTransform.forward * distanceFromCameraInMeter;
                animalPos.y = 0;
                animal.transform.position = animalPos;

                // https://stackoverflow.com/questions/22696782/placing-an-object-in-front-of-the-camera
                var animalRotation = new Quaternion(0.0f, cameraOrEyeTransform.transform.rotation.y, 0.0f, cameraOrEyeTransform.transform.rotation.w).eulerAngles;
                //animalRotation = Quaternion.Inverse(animalRotation);
                animal.transform.rotation = Quaternion.Euler(animalRotation.x + 180, animalRotation.y, animalRotation.z + 180);
            }

            i += 1;
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
