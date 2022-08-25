using Synchrony;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    }

    void Update()
    {
        // rotate animal (A: button on contrller or P on keyboard)
        if (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P))
        {
            if (activeAnimalIndex >= animalsToRotate.Count - 1)
                activeAnimalIndex = 0;
            else
                activeAnimalIndex += 1;

            ActivateActiveAnimal();
        }

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
                    distanceFromCameraInMeter = 3f;

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
}
