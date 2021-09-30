using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TextSpeech;
using UnityEngine.Events;
using UnityEngine.Android;
using Synchrony;
using System;
using TMPro;


// https://www.youtube.com/watch?v=XRXbVtr1fog
public class VoiceController : MonoBehaviour
{
    const string DEFAULT_LANGUAGE_CODE = "nl-NL"; // TODO

    public UnityEvent<string> PartialSpeechResultEvent = new UnityEvent<string>();

    #if UNITY_ANDROID
    private SpeechRecognizer androidSpeechRecognizer = null;
    #endif

    private void Start()
    {
        "StartTextSpeech".Log();

        #if UNITY_ANDROID
        // Create a Android SpeechRecognizer as a child of this VoiceController
        var speechRecognizerGameObject = new GameObject("SpeechRecognizer", components: new System.Type[] { typeof(SpeechRecognizer) });
        speechRecognizerGameObject.transform.parent = gameObject.transform;
        androidSpeechRecognizer = speechRecognizerGameObject.GetComponent<SpeechRecognizer>();
        
        #endif

#if UNITY_IPHONE
        TextToSpeech.instance.Setting(DEFAULT_LANGUAGE_CODE, _pitch: 1, _rate: 1);
        SpeechToText.instance.Setting(DEFAULT_LANGUAGE_CODE);
#elif UNITY_ANDROID
        androidSpeechRecognizer.language = DEFAULT_LANGUAGE_CODE;
#endif

#if UNITY_ANDROID
        //SpeechToText.instance.onPartialResultsCallback = OnPartialSpeechResult;
#endif
        SpeechToText.instance.onResultCallback = OnFinalSpeechResult;
        TextToSpeech.instance.onStartCallBack = OnSpeakStart;
        TextToSpeech.instance.onDoneCallback = OnSpeakStop;
        CheckListeningPermission();
    }

    void CheckListeningPermission()
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
#if UNITY_IPHONE
        TextToSpeech.instance.StartSpeak(message);
#endif
    }

    public void StopSpeaking()
    {
#if UNITY_IPHONE
    TextToSpeech.instance.StopSpeak();
#endif
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
#if UNITY_IPHONE
        SpeechToText.instance.StartRecording();
#elif UNITY_ANDROID
        androidSpeechRecognizer.StartListening();
#endif
    }

    public void StopListening()
    {
#if UNITY_IPHONE
        SpeechToText.instance?.StopRecording();
#elif UNITY_ANDROID
        androidSpeechRecognizer.StopListening();
#endif
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

}
