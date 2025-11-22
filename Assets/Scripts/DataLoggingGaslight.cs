using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataLoggingGaslight : MonoBehaviour
{
    [SerializeField] private string logFileName = "gaslightUserStudyLog";
    
    private List<float> runTimes = new List<float>();
    private List<string> runStartTimes = new List<string>();
    
    [System.Serializable]
    private struct ParticipantBlock
    {
        public int participantId;
        public List<float> times;
        public List<string> startTimes;

        public ParticipantBlock(int participantId, List<float> times, List<string> startTimes)
        {
            this.participantId = participantId;
            this.times = times;
            this.startTimes = startTimes;
        }
    }

    public void AddTimestamp()
    {
        runStartTimes.Add(DateTime.Now.ToString("yyyy-MM-ddTHH.mm.ss"));
    }
    
    public void AddTime(float time)
    {
        runTimes.Add(time);
    }

    public void OnBlockFinished(string blockType, int participantId)
    {
        WriteDataLog(blockType, participantId);
        runTimes.Clear();
        runStartTimes.Clear();
    }

    private void WriteDataLog(string blockType, int participantId)
    {
        var data = new ParticipantBlock(participantId, runTimes, runStartTimes);
        var path = GetNextFileName(participantId);
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(path, $"{{" +
                                $"\"time\": \"{DateTime.Now.ToString("yyyy-MM-ddTHH.mm.ss")}\"," +
                                $"\"blockType\": \"{blockType}\"," + json.Replace("{", "").Replace("}", "") + "}");
        
        Debug.LogWarning("Wrote data log to the following path: " + path);
    }

    private string GetNextFileName(int participantId)
    {
        var files = new DirectoryInfo(Application.persistentDataPath).GetFiles();
        var participantBaseLogFile = logFileName + "_" + participantId;
        var fileCount = files.Count(file => file.Name.StartsWith(participantBaseLogFile));
        
        return Application.persistentDataPath + "/" + participantBaseLogFile + "_" + fileCount + ".json";
    }
}