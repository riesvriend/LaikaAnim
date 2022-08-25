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

    public List<GameObject> animalToggles = new List<GameObject>();

    public ToggleGroup animalToggleGroup;
    public Toggle HorseToggle;
    public GameObject animalTogglePrefab;

    public AudioSource musicAudioSource;
    public VideoPlayer videoPlayer;

    //enum Sound : int { Silent, VideoOnly, MusicOnly, MusicAndVideo, MaxEnumValue };
    //private Sound currentSoundIndex = Sound.MusicAndVideo;
    public bool playBackgroundMusic = true;

    private int movieIndex = 0;

    private void Awake()
    {
        ActivateActiveAnimal();
        ActivateSound();

        // The first toggle button is the dummy for the prefab; disable it
        var dummy = animalToggleGroup.GetComponentsInChildren<Toggle>().Single();
        dummy.gameObject.SetActive(false); // somehow this will carry forward to the prefab

        // Generate toggle buttons from a prefab, one for each item in animalsToRotate
        var index = 0;
        foreach (var animal in animalsToRotate)
        {
            var animalToggleGameObject = Instantiate(animalTogglePrefab, parent: animalToggleGroup.transform);
            animalToggleGameObject.SetActive(true); // so we have to re-anable it in spawned objects
            var label = animalToggleGameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Single();
            label.text = animal.name;

            var animalToggle = animalToggleGameObject.GetComponent<Toggle>();
            animalToggle.SetIsOnWithoutNotify(index == activeAnimalIndex);

            animalToggle.onValueChanged.AddListener((isOn) => AnimalToggleValueChanged(animalToggle, isOn, animal));
        }
    }

    private Toggle[] AnimalToggles()
    {
        return animalToggleGroup.GetComponentsInChildren<Toggle>();
    }

    private void OnDestroy()
    {
        //foreach (var t in AnimalToggles())
        //    t.onValueChanged.RemoveAllListeners();
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

    private void ActivateActiveAnimal()
    {
        var i = 0;
        foreach (var animal in animalsToRotate)
        {
            var isActive = activeAnimalIndex == i;
            animal.SetActive(isActive);
            if (isActive && cameraOrEyeTransform != null)
            {
                float distanceFromCameraInMeter = 1.5f;

                if (i == 1)
                    // hack: horse is larger
                    distanceFromCameraInMeter = 4f;

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



    public void AnimalSelectedHandler(PointerEventData eventData)
    {
        "AnimalSelectedHandler {eventData}".Log();
    }
}
