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
    const string LANG_CODE = "nl-NL"; // TODO

    public UnityEvent<string> PartialSpeechResultEvent = new UnityEvent<string>();

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
        $"StartSpeaking '{message}'".Log();
        TextToSpeech.instance.StartSpeak(message);
    }

    public void StopSpeaking()
    {
        TextToSpeech.instance.StopSpeak();
    }

    void OnSpeakStart()
    {
        "OnSpeakStart".Log();
    }
    void OnSpeakStop()
    {
        "OnSpeakStop".Log();
    }
    #endregion

    #region SpeechToText
    public void StartListening()
    {
        SpeechToText.instance.StartRecording();
    }

    public void StopListening()
    {
        SpeechToText.instance?.StopRecording();
    }


    void OnFinalSpeechResult(string result)
    {
        $"OnFinalSpeechResult: ${result}".Log();
        
        PartialSpeechResultEvent?.Invoke(result);
    }

    void OnPartialSpeechResult(string result)
    {
        $"OnPartialSpeechResult: ${result}".Log();

        PartialSpeechResultEvent?.Invoke(result);
    }

    #endregion

    void Setup(string language)
    {
        TextToSpeech.instance.Setting(language, _pitch: 1, _rate: 1);
        SpeechToText.instance.Setting(language);
    }
}
