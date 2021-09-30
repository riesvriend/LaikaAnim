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

    private void onTextCommandSubmit(string text)
    {
        $"onTextCommandSubmit: {text}".Log();
    }

    private void OnDisable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
