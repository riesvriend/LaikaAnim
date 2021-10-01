using Synchrony;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarpetUIScript : MonoBehaviour
{
    [SerializeField] public TMP_InputField textCommandInputField = null;

    private void OnEnable()
    {
        textCommandInputField?.onSubmit.AddListener(onTextCommandSubmit);
    }

    private void OnDisable()
    {
        textCommandInputField?.onSubmit.RemoveListener(onTextCommandSubmit);
    }

    private void onTextCommandSubmit(string text)
    {
        $"onTextCommandSubmit: {text}".Log();

        var laikaMovementHandler = FindObjectOfType<LaikaMovement>();
        laikaMovementHandler?.HandleVoiceCommand(text);
    }
}
