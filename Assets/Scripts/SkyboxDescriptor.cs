using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(
    menuName = "Vriend/New Skybox Descriptor",
    order = -100,
    fileName = "New Skybox Descriptor"
)]
public class SkyboxDescriptor : ScriptableObject
{
    [Tooltip("Clip to play as the skybox video")]
    public VideoClip videoClip;

    [Tooltip("HDRI image to display as the skybox video")]
    public Texture hdriCubeMap;

    [Tooltip(
        "Rotation to align the sun in the video or texture with the Directional Light in the scene"
    )]
    public float rotation;

    [Tooltip("Audio to play with the video or image")]
    public AudioClip audio;
}
