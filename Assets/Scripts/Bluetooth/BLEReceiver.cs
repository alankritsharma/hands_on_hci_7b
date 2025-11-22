using System;
using System.Collections.Generic;
using Shatalmic;
using TMPro;
using UnityEngine;
using static Shatalmic.Networking;

public class BLEReceiver : MonoBehaviour
{
    [SerializeField] private PMManager pmManager;
    [SerializeField] private AbstractModelManager modelManager;
    [SerializeField] private NoiseManager noiseManager;
    [SerializeField] private AlignmentManager alignmentManager;
    [SerializeField] private TMP_Text debugLog;
    [SerializeField] private TMP_Dropdown pmDropdown;
    [SerializeField] private GameObject canvas;

    private CustomNetworking bleNetworking;
    private List<NetworkDevice> remoteDevices;

    public static BLEReceiver Instance { get; private set; }

    void Start()
    {
        if (BLEReceiver.Instance == null)
        {
            BLEReceiver.Instance = this;
            this.Init();
        }
        else Debug.LogError("Second BLEReceiver tried to init.");
    }

    private void Init()
    {
        this.ClearDebugLog();
        this.bleNetworking = this.GetComponent<CustomNetworking>();
        this.remoteDevices = new List<NetworkDevice>();
        this.bleNetworking.Initialize(this.OnBLEError, this.OnStatusMessage);
    }

    private void Update()
    {
        // press left joystick to toggle the menu visibility
        if (OVRInput.GetUp(ButtonMapping.StickL, OVRInput.Controller.Touch))
        {
            Utils.ToggleGameObject(this.canvas);
        }
    }

    public void ClearDebugLog()
    {
        this.debugLog.text = "";
    }

    private void PrintDebugLine(string message)
    {
        this.debugLog.text += $"{message}\n";
    }

    public void PreviousLogPage()
    {
        if (this.debugLog.pageToDisplay > 0) this.debugLog.pageToDisplay--;
    }

    public void NextLogPage()
    {
        this.debugLog.pageToDisplay++;
    }

    public void StartServer()
    {
        Debug.Log("Starting Server...");
        this.PrintDebugLine("Starting Server...");
        this.bleNetworking.StartServer(BLEData.defaultServerName, this.OnRemoteDeviceConnected, this.OnRemoteDeviceDisconnected, this.OnRemoteInputBytes, this.OnRemoteInputString);
    }

    public void StopServer()
    {
        Debug.Log("Stopping Server...");
        this.PrintDebugLine("Stopping Server...");
        this.bleNetworking.StopServer(this.OnServerStopped);
    }

    private void OnBLEError(string error)
    {
        BluetoothLEHardwareInterface.Log($"BLE Error: {error}");
        Debug.LogError($"BLE Error: {error}");
        this.PrintDebugLine($"BLE Error: {error}");
    }

    private void OnStatusMessage(string message)
    {
        BluetoothLEHardwareInterface.Log($"BLE Status Message: {message}");
        Debug.Log($"BLE Status Message: {message}");
        this.PrintDebugLine($"BLE Status Message: {message}");
    }

    private void OnRemoteDeviceConnected(NetworkDevice networkDevice)
    {
        Debug.Log($"Remote device connected: {networkDevice.Name}");
        this.PrintDebugLine($"Remote device connected: {networkDevice.Name}");
        this.remoteDevices.Add(networkDevice);

        this.bleNetworking.WriteDevice(networkDevice, new byte[] { BLEData.S2R_byteConnected, Convert.ToByte(this.pmManager.GetPMCount()) }, () => { });
    }

    private void OnRemoteDeviceDisconnected(NetworkDevice networkDevice)
    {
        Debug.Log($"Remote device disconnected: {networkDevice.Name}");
        this.PrintDebugLine($"Remote device disconnected: {networkDevice.Name}");
        this.remoteDevices.Remove(networkDevice);
    }

    private void OnRemoteInputBytes(NetworkDevice remoteDevice, string characteristic, byte[] data)
    {
        if (data == null || data.Length == 0) return;

        Debug.Log($"Remote {remoteDevice.Name} sent byte data: {data[0].ToString()}");
        this.PrintDebugLine($"Remote {remoteDevice.Name} sent byte input: {data[0].ToString()}");

        if (data[0] == BLEData.R2S_byteRequestPMName)
        {
            int desiredIndex = data[1];
            this.bleNetworking.WriteDeviceString(remoteDevice, $"{desiredIndex};{this.pmManager.GetPMName(desiredIndex)}", () => { });
        }
        else if (data[0] == BLEData.R2S_byteSetModelActive)
        {
            if (this.modelManager.IsModelVisible() != (data[1] == 1)) this.modelManager.ToggleModelVisibility(data[2] == 1);
        }
        else if (data[0] == BLEData.R2S_byteSetNoiseActive)
        {
            if (this.noiseManager.GetNoiseVolumeActive() != (data[1] == 1)) this.noiseManager.ToggleNoiseVolume();
        }
        else if (data[0] == BLEData.R2S_byteStopAndRealign)
        {
            StartCoroutine(this.pmManager.StopPM(0, 0)); // currently just uses iteration 0 to get the bool for resolving or not, should probably be improved
            this.alignmentManager.Realign(false);
        }
        else if (data[0] == BLEData.R2S_byteChooseActivePM)
        {
            this.pmDropdown.value = data[1]; // relies on the listeners specified in PMManager or PMSandboxManager, should probably be independent at some point
        }
        else if (data[0] == BLEData.R2S_byteSetPMActive)
        {
            if (data[1] == 1) StartCoroutine(this.pmManager.StartPM(0));
            else StartCoroutine(this.pmManager.StopPM(0, 0)); // currently just uses iteration 0 to get the bool for resolving or not, should probably be improved
        }
        else if (data[0] == BLEData.R2S_byteShowNextPainting)
        {
            // TODO: add fields for both StudyManager and PMSandboxManager and call ShowNextPainting on the one that is not null? Maybe there is a more elegant way
        }
    }

    private void OnRemoteInputString(NetworkDevice remoteDevice, string characteristic, string data)
    {
        if (data == null || data == "") return;

        Debug.Log($"Remote {remoteDevice.Name} sent string data: {data}");
        this.PrintDebugLine($"Remote {remoteDevice.Name} sent string input: {data}");
    }

    private void OnServerStopped()
    {
        Debug.Log("Server stopped");
        this.PrintDebugLine("Server stopped");
    }

    public void SendStateUpdate()
    {
        byte modelVisible = this.modelManager.IsModelVisible() ? (byte)1 : (byte)0;
        byte noiseActive = this.noiseManager.GetNoiseVolumeActive() ? (byte)1 : (byte)0;
        byte activePM = (byte)this.pmManager.GetChosenConfigIndex();

        foreach (NetworkDevice nd in this.remoteDevices) this.bleNetworking.WriteDevice(nd, new byte[] { BLEData.S2R_byteUpdateState, modelVisible, noiseActive, activePM}, () => { });
    }
}
