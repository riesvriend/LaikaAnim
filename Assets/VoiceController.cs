using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using UnityEngine.Events;
using UnityEngine.Android;
using Synchrony;
using System;


// https://www.youtube.com/watch?v=XRXbVtr1fog
public class VoiceController : MonoBehaviour
{
    const string LANG_CODE = "en-US";

    UnityEvent translatedSpeech = new UnityEvent();

    private void Start()
    {
        StartTextSpeech();
    }

    public void StartTextSpeech()
    {
        "StartTextSpeech".Log();

        Setup(LANG_CODE);

#if UNITY_ANDROID
        SpeechToText.instance.onPartialResultsCallback = OnPartialSpeechResult;
#endif
        SpeechToText.instance.onResultCallback = OnFinalSpeechResult;
        TextToSpeech.instance.onStartCallBack = OnSpeakStart;
        TextToSpeech.instance.onDoneCallback = OnSpeakStop;
        CheckPermission();
    }

    void CheckPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            Permission.RequestUserPermission(Permission.Microphone);
#endif
    }

    #region TextToSpeech
    public void StartSpeaking(string message)
    {
        TextToSpeech.instance.StartSpeak(message);
    }

    public void StopSpeaking()
    {
        TextToSpeech.instance.StopSpeak();
    }

    void OnSpeakStart()
    {
        Debug.Log("OnSpeakStart");
    }
    void OnSpeakStop()
    {
        Debug.Log("OnSpeakStop");
    }
    #endregion

    #region SpeechToText
    public void StartListening()
    {
        SpeechToText.instance.StartRecording();
    }
    public void StopListening()
    {
        SpeechToText.instance.StopRecording();
    }


    void OnFinalSpeechResult(string result)
    {
        Debug.Log($"OnFinalSpeechResult: ${result}");
    }

    void OnPartialSpeechResult(string result)
    {
        Debug.Log($"OnPartialSpeechResult: ${result}");
    }

    #endregion

    void Setup(string language)
    {
        TextToSpeech.instance.Setting(language, _pitch: 1, _rate: 1);
        SpeechToText.instance.Setting(language);
    }
}
