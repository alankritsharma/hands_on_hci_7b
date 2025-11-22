using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class NatNetBridgeReceiver: MonoBehaviour
    {
        [SerializeField] private TMP_Text statusIndicator;
        [SerializeField] private bool runHeadless;
        
        private UdpClient client;
        private IPEndPoint remoteEndPoint;
        private const int Port = 5005;
        private bool relayMovement;
        private bool initialized;
        
        private Vector3 initialOptiPos;
        private Quaternion initialOptiRot;

        private Vector3 initialUnityPos;
        private Quaternion initialUnityRot;

        public bool lookingForFastSwitch = false;
        public static event Action PositionChanged ;

        private void Start()
        {
            client = new UdpClient(Port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);
            relayMovement = false;
            initialized = false;
        }

        private void Update()
        {
            if (client.Available > 0)
            {
                byte[] data = client.Receive(ref remoteEndPoint);
                // if (data.Length < 32) return;
                // if (relayMovement) ParseData(data);
                if (data.Length > 0)
                    if (runHeadless) PositionChanged?.Invoke();
 					if (data[0] != 0 && relayMovement && lookingForFastSwitch) PositionChanged?.Invoke();
            }
        }

        // private void ParseData(byte[] data)
        // {
        //     // Format: id,x,y,z,qx,qy,qz,qw
        //     
        //     int id = BitConverter.ToInt32(data, 0);
        //     float x = BitConverter.ToSingle(data, 4);
        //     float y = BitConverter.ToSingle(data, 8);
        //     float z = BitConverter.ToSingle(data, 12);
        //     float qx = BitConverter.ToSingle(data, 16);
        //     float qy = BitConverter.ToSingle(data, 20);
        //     float qz = BitConverter.ToSingle(data, 24);
        //     float qw = BitConverter.ToSingle(data, 28);
        //     
        //     this.TrackPosition(id, -x, y, z, qz, qy, qx, -qw);
        // }
        //
        // private void TrackPosition(int id, float x, float y, float z, float qx, float qy, float qz, float qw)
        // {
        //     // if the base positions are not initialized yet, set them
        //     if (!initialized)
        //     {
        //         initialOptiPos = new Vector3(x, y, z);
        //         initialOptiRot = new Quaternion(qx, qy, qz, qw);
        //         
        //         initialUnityPos = this.transform.position;
        //         initialUnityRot = this.transform.rotation;
        //         
        //         initialized = true;
        //     }
        //
        //     // only move the object by deltas
        //     var delta = new Vector3(x, y, z) - initialOptiPos;
        //     if (lookingForFastSwitch && delta.magnitude > 0.01f)
        //     {
        //         PositionChanged?.Invoke();
        //     }
        //     transform.position = initialUnityPos + delta;
        //     
        //     // Rotation delta
        //     var deltaRot =  new Quaternion(qx, qy, qz, qw) * Quaternion.Inverse(initialOptiRot);
        //     transform.rotation = deltaRot * initialUnityRot;
        // }

        public void ToggleRelayMovement()
        {
            this.relayMovement = !this.relayMovement;
            initialized = false;
            statusIndicator.color = this.relayMovement ? Color.green : Color.red;
        }
        
        private void OnApplicationQuit()
        {
            client.Close();
        }
    }
}