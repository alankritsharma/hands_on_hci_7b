using System;
using UnityEngine;
using TMPro;

namespace DefaultNamespace
{
    

    public class ConsoleLogger : MonoBehaviour
    {
        public TextMeshProUGUI logText;
        private string logOutput = "";
        
        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception)
            {
                logOutput += $"{type}: {logString} | {stackTrace}\n";
                logText.text = logOutput;
            }
        }
    }

}