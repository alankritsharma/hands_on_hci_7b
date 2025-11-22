using UnityEngine;

public static class BLEData
{
    // connection
    public static string defaultServerName = "SwitchAR_HMD";
    public static string defaultClientName = "Remote0";

    // communication bytes - remote/client sending to server
    /// <summary>Second byte contains the index of the desired PM</summary>
    public static byte R2S_byteRequestPMName = 0;
    /// <summary>Second byte contains desired model state (0 hidden, 1 visible), third byte fade mode (0 instant, 1 fade)</summary>
    public static byte R2S_byteSetModelActive = 1;
    /// <summary>Second byte contains 0 or 1 to indicate if the noise should be active</summary>
    public static byte R2S_byteSetNoiseActive = 2;
    /// <summary>Stops the current PM and realigns the model</summary>
    public static byte R2S_byteStopAndRealign = 3;
    /// <summary>Second byte contains the index of the desired PM</summary>
    public static byte R2S_byteChooseActivePM = 4;
    /// <summary>Second byte contains 0 or 1 to indicate if the currently selected PM should be active</summary>
    public static byte R2S_byteSetPMActive = 5;
    /// <summary>Allow triggering the next painting in case the user struggles with the progress bar</summary>
    public static byte R2S_byteShowNextPainting = 6;


    // communication bytes - server sending to remote/client
    /// <summary>Indicate that the server successfully connected to the remote, second byte contains the number of available PMs</summary>
    public static byte S2R_byteConnected = 0;
    /// <summary>Second byte contains the model state (0 hidden, 1 visible), third byte contains the noise state (0 hidden, 1 visible), fourth byte the active PM</summary>
    public static byte S2R_byteUpdateState = 1;
}