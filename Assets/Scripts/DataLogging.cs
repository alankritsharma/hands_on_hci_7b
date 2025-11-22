using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class DataLogging : MonoBehaviour
{
    [SerializeField] private string logFileName = "mscUserStudyLog";

    private List<Vector3> positions = new List<Vector3>();

    [System.Serializable]
    private struct IterationData
    {
        public List<Vector3> paintingPositions;
        public int iterationIndex;

        public IterationData(List<Vector3> list, int index)
        {
            this.paintingPositions = list;
            this.iterationIndex = index;
        }
    }

    public void AddPosition(Vector3 position)
    {
        this.positions.Add(position);
    }

    public void OnIterationFinished(int iterationIndex)
    {
        this.WriteDataLog(iterationIndex);
        this.positions.Clear();
    }

    public void WriteDataLog(int iterationIndex)
    {
        IterationData data = new IterationData(this.positions, iterationIndex);
        string path = this.GetNextFileName();
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);

        Debug.LogWarning("Wrote data log to the following path: " + path);
    }

    private string GetNextFileName()
    {
        FileInfo[] files = new DirectoryInfo(Application.persistentDataPath).GetFiles();
        int fileCount = 0;
        foreach (FileInfo file in files) if (file.Name.StartsWith(this.logFileName)) fileCount++;

        return Application.persistentDataPath + "/" + this.logFileName + fileCount.ToString() + ".json";
    }
}