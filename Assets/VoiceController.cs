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
    const string DEFAULT_LANGUAGE_CODE = "nl-NL"; // TODO

    public UnityEvent<string> SpeechResultEvent = new UnityEvent<string>();

    #if UNITY_ANDROID
    private SpeechRecognizer androidSpeechRecognizer = null;
    #endif

    private void Awake()
    {
        "StartTextSpeech".Log();

        /*
        10-01 17:24:48.174 27411 27411 D AndroidRuntime: Shutting down VM
        10-01 17:24:48.175 28276 29162 W native  : W1001 17:24:48.175004   29162 hybrid_selector_impl.cc:141] No active session.
        10-01 17:24:48.175  1024 13731 V APM_AudioPolicyManager: getOutputForAttrInt() attributes={ Content type: AUDIO_CONTENT_TYPE_MUSIC Usage: AUDIO_USAGE_NOTIFICATION_EVENT Source: -1 Flags: 0x800 Tags:  } stream=AUDIO_STREAM_NOTIFICATION session 2801 selectedDeviceId 0
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: FATAL EXCEPTION: main
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: Process: studio.synchrony.Livelier, PID: 27411
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: java.lang.IllegalArgumentException: Service not registered: android.speech.SpeechRecognizer$Connection@67f7e1c
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.app.LoadedApk.forgetServiceDispatcher(LoadedApk.java:1901)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.app.ContextImpl.unbindService(ContextImpl.java:1954)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.content.ContextWrapper.unbindService(ContextWrapper.java:810)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.speech.SpeechRecognizer.destroy(SpeechRecognizer.java:411)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at com.example.eric.unityspeechrecognizerplugin.SpeechRecognizerFragment.StopListening(SpeechRecognizerFragment.java:143)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at com.example.eric.unityspeechrecognizerplugin.SpeechRecognizerFragment.RestartListening(SpeechRecognizerFragment.java:148)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at com.example.eric.unityspeechrecognizerplugin.SpeechRecognizerFragment$SpeechRecognitionListener.onError(SpeechRecognizerFragment.java:202)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.speech.SpeechRecognizer$InternalListener$1.handleMessage(SpeechRecognizer.java:453)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.os.Handler.dispatchMessage(Handler.java:106)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.os.Looper.loop(Looper.java:247)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at android.app.ActivityThread.main(ActivityThread.java:8618)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at java.lang.reflect.Method.invoke(Native Method)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at com.android.internal.os.RuntimeInit$MethodAndArgsCaller.run(RuntimeInit.java:602)
        10-01 17:24:48.175 27411 27411 E AndroidRuntime: 	at com.android.internal.os.ZygoteInit.main(ZygoteInit.java:1130)
        10-0

        TODO: to prevent above error, keep the SpeechToText object running as a child of App. But then iOS plugin can't find it by name?

         */

        var speechToText = new GameObject("SpeechToText", components: new System.Type[] { typeof(TextSpeech.SpeechToText) });
        speechToText.transform.parent = gameObject.transform;
        
        var textToSpeech = new GameObject("TextToSpeech", components: new System.Type[] { typeof(TextSpeech.TextToSpeech) });
        textToSpeech.transform.parent = gameObject.transform;

#if UNITY_ANDROID
        // Create a Android SpeechRecognizer as a child of this VoiceController
        var speechRecognizerGameObject = new GameObject("SpeechRecognizer", components: new System.Type[] { typeof(SpeechRecognizer) });
        speechRecognizerGameObject.transform.parent = gameObject.transform;
        androidSpeechRecognizer = speechRecognizerGameObject.GetComponent<SpeechRecognizer>();
        androidSpeechRecognizer.TextSpoken.AddListener(OnFinalSpeechResult);
        androidSpeechRecognizer.language = DEFAULT_LANGUAGE_CODE;
#endif

#if UNITY_IPHONE
        TextToSpeech.instance.Setting(DEFAULT_LANGUAGE_CODE, _pitch: 1, _rate: 1);
        SpeechToText.instance.Setting(DEFAULT_LANGUAGE_CODE);
        //SpeechToText.instance.onResultCallback = OnPartialSpeechResult;
        SpeechToText.instance.onResultCallback = OnFinalSpeechResult;
#endif
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
        $"OnFinalSpeechResult: {result}".Log();

        SpeechResultEvent.Invoke(result);
    }

    void OnPartialSpeechResult(string result)
    {
        $"OnPartialSpeechResult: {result}".Log();

        SpeechResultEvent.Invoke(result);
    }

#endregion
}
