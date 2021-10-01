using UnityEngine;

public class SpeechRecognizerPlugin_Android : SpeechRecognizerPlugin
{
    public SpeechRecognizerPlugin_Android(string gameObjectName) : base(gameObjectName) { }

    private string javaClassPackageName = "com.example.eric.unityspeechrecognizerplugin.SpeechRecognizerFragment";
    private AndroidJavaClass javaClass = null;
    private AndroidJavaObject instance = null;
    // The java object gives a null ref if you stop listening twice...
    private bool isListening = false;

    protected override void SetUp()
    {
        Debug.Log("SetUpAndroid " + gameObjectName);
        javaClass = new AndroidJavaClass(javaClassPackageName);
        javaClass.CallStatic("SetUp", gameObjectName);
        instance = javaClass.GetStatic<AndroidJavaObject>("instance");
    }

    public override void StartListening()
    {
        StartListening(this.isContinuousListening, this.language, this.maxResults);
    }

    public override void StartListening(bool isContinuous = false, string newLanguage = "en-US", int newMaxResults = 10)
    {
        if (!isListening)
        {
            instance.Call("StartListening", isContinuous, language, maxResults);
            isListening = true;
        }
    }

    public override void StopListening()
    {
        if (isListening)
        {
            instance.Call("StopListening");
            isListening = false;
        }
    }

    public override void SetContinuousListening(bool isContinuous)
    {
        this.isContinuousListening = isContinuous;
        instance.Call("SetContinuousListening", isContinuous);
    }

    public override void SetLanguageForNextRecognition(string newLanguage)
    {
        this.language = newLanguage;
        instance.Call("SetLanguage", newLanguage);
    }

    public override void SetMaxResultsForNextRecognition(int newMaxResults)
    {
        this.maxResults = newMaxResults;
        instance.Call("SetMaxResults", newMaxResults);
    }
}