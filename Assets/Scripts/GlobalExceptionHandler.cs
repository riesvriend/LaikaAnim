using Synchrony;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalExceptionHandler : MonoBehaviour
{
    void Awake()
    {
        Application.logMessageReceived += HandleException;
        //DontDestroyOnLoad(gameObject);
    }

    void HandleException(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            logString.Log();
            stackTrace.Log();
        }
    }
}
