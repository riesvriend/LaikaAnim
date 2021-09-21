using Synchrony;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SpeechRecognizerPlugin;

/// <summary>
/// https://github.com/EricBatlle/UnityAndroidSpeechRecognizer
/// </summary>
public class SpeechRecognizer : MonoBehaviour, ISpeechRecognizerPlugin
{
    //[SerializeField] private Button startListeningBtn = null;
    //[SerializeField] private Button stopListeningBtn = null;
    //[SerializeField] private Toggle continuousListeningTgle = null;
    //[SerializeField] private TMP_InputField maxResultsInputField = null;
    //[SerializeField] private TextMeshProUGUI resultsTxt = null;
    //[SerializeField] private TextMeshProUGUI errorsTxt = null;

    [SerializeField] public string language = "en-US";
    private SpeechRecognizerPlugin plugin = null;

    private void Start()
    {
        plugin = SpeechRecognizerPlugin.GetPlatformPluginVersion(this.gameObject.name);
        plugin.SetLanguageForNextRecognition(language);
        plugin.SetContinuousListening(isContinuousListening: false);
        plugin.SetMaxResultsForNextRecognition(10);
        
        //startListeningBtn.onClick.AddListener(StartListening);
        //stopListeningBtn.onClick.AddListener(StopListening);
        //continuousListeningTgle.onValueChanged.AddListener(SetContinuousListening);
        //languageDropdown.onValueChanged.AddListener(SetLanguage);
        //maxResultsInputField.onEndEdit.AddListener(SetMaxResults);
    }

    public void StartListening()
    {
        plugin.StartListening();
    }

    public void StopListening()
    {
        plugin.StopListening();
    }

    //private void SetLanguage(int dropdownValue)
    //{
    //    string newLanguage = languageDropdown.options[dropdownValue].text;
    //    plugin.SetLanguageForNextRecognition(newLanguage);
    //}

    private void SetMaxResults(string inputValue)
    {
        if (string.IsNullOrEmpty(inputValue))
            return;

        int maxResults = int.Parse(inputValue);
        plugin.SetMaxResultsForNextRecognition(maxResults);
    }

    public void OnResult(string recognizedResult)
    {
        char[] delimiterChars = { '~' };
        string[] result = recognizedResult.Split(delimiterChars);

        var text = "";
        for (int i = 0; i < result.Length; i++)
            text += result[i] + Environment.NewLine;
        text.Log();
    }

    public void OnError(string recognizedError)
    {
        ERROR error = (ERROR)int.Parse(recognizedError);
        switch (error)
        {
            case ERROR.UNKNOWN:
                "SpeechRecognizer ERROR: Unknown".Log();
                break;
            case ERROR.INVALID_LANGUAGE_FORMAT:
                "SpeechRecognizer ERROR: Language format is not valid".Log();
                break;
            default:
                $"SpeechRecognizer ERROR: ${recognizedError}".Log();
                break;
        }
    }
}
