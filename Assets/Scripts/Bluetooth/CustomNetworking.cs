using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shatalmic;
using UnityEngine;

public class CustomNetworking : Networking
{
    [SerializeField] private int desiredMTU = 128;
    [SerializeField] private int stringCharacteristicSize = 128;
    [SerializeField] private bool verbose;

    private Characteristic StringCharacteristic = new Characteristic { ServiceUUID = "37200001-7638-4216-B629-96AD40F79AA1", CharacteristicUUID = "1CF3F7DA-39E3-4D60-8937-58F954BE82E2", Found = false };
    private Action<NetworkDevice, string, byte[]> OnDeviceDataBytes;
    private Action<NetworkDevice, string, string> OnDeviceDataString;

    private int subscribedCharacteristics;

    private new void Start()
    {
        base.Start();

        Networking.Characteristics.Add(this.StringCharacteristic);
    }

    private new void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        if (_deviceToConnect == null)
                        {
                            if (this.verbose) StatusMessage = "Can connect";
                            _deviceToConnect = NetworkDeviceList.Where(d => !d.Connected).Select(d => d).FirstOrDefault();
                            if (_deviceToConnect != null)
                            {
                                if (this.verbose) StatusMessage = string.Format("Need connect: {0}", _deviceToConnect.Name);
                                SetState(States.Connect, 0.1f);
                            }
                        }
                        break;

                    case States.StartScan:
                        SetState(States.RestartScan, 5f);
                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, deviceName) =>
                        {
                            if (deviceName.StartsWith(_networkName))
                            {
                                StatusMessage = "Found " + address;

                                if (NetworkDeviceList == null)
                                    NetworkDeviceList = new List<NetworkDevice>();

                                var checkForDevice = NetworkDeviceList.Where(d => d.Name.Equals(deviceName)).Select(d => d).FirstOrDefault();
                                if (checkForDevice == null)
                                {
                                    NetworkDeviceList.Add(new NetworkDevice { Name = deviceName, Address = address, Connected = false });

                                    if (_deviceToConnect == null)
                                        SetState(States.None, 0.01f);
                                }
                            }

                        });
                        break;

                    case States.RestartScan:
                        StatusMessage = "Restarting scanning";
                        BluetoothLEHardwareInterface.StopScan();
                        SetState(States.StartScan, 0.01f);
                        break;

                    case States.Connect:
                        if (_deviceToConnect != null)
                        {
                            StatusMessage = string.Format("Connecting to {0}, {1}...", _deviceToConnect.Name, _deviceToConnect.Address);

                            BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceToConnect.Address, (deviceAddress) =>
                            {
                                if (this.verbose) StatusMessage = string.Format("Connect to {0}...", deviceAddress);
                            }, (serviceDeviceAddress, serviceDeviceUUID) =>
                            {
                                if (this.verbose) StatusMessage = string.Format("Service Discovered {0}, {1}...", serviceDeviceAddress, serviceDeviceUUID);
                            }, (address, serviceUUID, characteristicUUID) =>
                            {
                                if (this.verbose) StatusMessage = string.Format("Characteristic Discovered {0}, {1}, {2}...", address, serviceUUID, characteristicUUID);
                                var characteristic = GetCharacteristic(serviceUUID, characteristicUUID);
                                if (characteristic != null)
                                {
                                    if (this.verbose) StatusMessage = string.Format("A characteristic was found {0}", characteristicUUID);
                                    characteristic.Found = true;

                                    if (AllCharacteristicsFound)
                                    {
                                        StatusMessage = string.Format("All characteristics found");
                                        _deviceToConnect.Connected = true;
                                        SetState(States.RequestMTU, 2f);
                                    }
                                }
                            }, (disconnectAddress) =>
                            {
                                var networkDevice = NetworkDeviceList.Where(d => d.Address.Equals(disconnectAddress)).Select(d => d).FirstOrDefault();
                                if (networkDevice != null)
                                {
                                    StatusMessage = "Disconnected from " + networkDevice.Name;
                                    if (OnDeviceDisconnected != null && OnDeviceDisconnected != null)
                                        OnDeviceDisconnected(networkDevice);

                                    NetworkDeviceList.Remove(networkDevice);
                                    StatusMessage = string.Format("1 device count: {0}", NetworkDeviceList.Count);
                                    if (networkDevice == _deviceToDisconnect)
                                    {
                                        _deviceToDisconnect = null;
                                        SetState(States.Disconnect, 0.1f);
                                    }
                                }
                            });
                        }
                        break;

                    case States.RequestMTU:
                        {
                            _state = States.None;
                            if (this.verbose) StatusMessage = "Request MTU";
                            BluetoothLEHardwareInterface.RequestMtu(_deviceToConnect.Address, this.desiredMTU, (deviceAddress, newMTU) =>
                            {
                                _mtu = newMTU - 3;

                                StatusMessage = $"MTU set to {_mtu}";
                                if (_mtu < this.stringCharacteristicSize) StatusMessage = $"WARNING: MTU smaller than the string characteristic size (MTU of {_mtu} vs. {this.stringCharacteristicSize})!";

                                SetState(States.WriteMTUToClient, 0.01f);
                            });
                        }
                        break;

                    case States.WriteMTUToClient:
                        {
                            _state = States.None;
                            if (this.verbose) StatusMessage = "Writing MTU to client";
                            BluetoothLEHardwareInterface.WriteCharacteristic(_deviceToConnect.Address, CommandCharacteristic.ServiceUUID, CommandCharacteristic.CharacteristicUUID, new byte[] { 0xA5, (byte)_mtu }, 2, true, (characteristicWritten) =>
                            {
                                if (this.verbose) StatusMessage = "MTU Written to client";
                                SetState(States.Subscribe, 0.05f);
                            });
                        }
                        break;

                    case States.Subscribe:
                        _state = States.None;
                        if (this.verbose) StatusMessage = "Subscribe to device";

                        // byte characteristic
                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceToConnect.Address, SampleCharacteristic.ServiceUUID, SampleCharacteristic.CharacteristicUUID,
                            (deviceAddressNotify, characteristicNotify) =>
                            {
                                this.CheckDeviceReady(deviceAddressNotify, characteristicNotify);
                                // string characteristic
                                BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceToConnect.Address, StringCharacteristic.ServiceUUID, StringCharacteristic.CharacteristicUUID,
                                    this.CheckDeviceReady, (deviceAddressData, characteristicData, bytes) =>
                                    {
                                        var networkDevice = NetworkDeviceList.Where(d => d.Address.Equals(deviceAddressData)).Select(d => d).FirstOrDefault();
                                        if (networkDevice != null && OnDeviceDataString != null)
                                            OnDeviceDataString(networkDevice, characteristicData, Encoding.ASCII.GetString(bytes));
                                    }
                                );
                            }, (deviceAddressData, characteristicData, bytes) =>
                            {
                                var networkDevice = NetworkDeviceList.Where(d => d.Address.Equals(deviceAddressData)).Select(d => d).FirstOrDefault();
                                if (networkDevice != null && OnDeviceDataBytes != null)
                                    OnDeviceDataBytes(networkDevice, characteristicData, bytes);
                            }
                        );
                        break;

                    case States.NextUpdateCharactersticValuePacket:
                        _state = States.None;
                        _sendFromClient();
                        break;

                    case States.Disconnect:
                        _deviceToDisconnect = NetworkDeviceList.Where(d => d.DoDisconnect).Select(d => d).FirstOrDefault();
                        if (_deviceToDisconnect != null)
                        {
                            SetState(States.Disconnecting, 5f);
                            if (_deviceToDisconnect.Connected)
                            {
                                BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceToDisconnect.Address, (address) =>
                                {
                                    // since we have a callback for disconnect in the connect method above, we don't
                                    // need to process the callback here.
                                });
                            }
                            else
                            {
                                NetworkDeviceList.Remove(_deviceToDisconnect);
                                StatusMessage = string.Format("2 device count: {0}", NetworkDeviceList.Count);

                                _deviceToDisconnect = null;
                                _state = States.None;
                            }
                        }
                        else
                        {
                            _state = States.None;
                            if (OnFinishedStoppingServer != null)
                            {
                                OnFinishedStoppingServer();
                                OnFinishedStoppingServer = null;
                            }

                            _serverStarted = false;
                        }
                        break;

                    case States.Disconnecting:
                        if (_deviceToDisconnect != null && NetworkDeviceList != null && NetworkDeviceList.Contains(_deviceToDisconnect))
                        {
                            StatusMessage = string.Format("3 device count: {0}", NetworkDeviceList.Count);

                            NetworkDeviceList.Remove(_deviceToDisconnect);
                            _deviceToDisconnect = null;
                        }

                        // if we got here we timed out disconnecting, so just go to disconnected state
                        SetState(States.Disconnect, 0.1f);
                        break;
                }
            }
        }
    }

    public void SetVerbose(bool verbose)
    {
        this.verbose = verbose;
    }

    private void CheckDeviceReady(string deviceAddressNotify, string characteristicNotify)
    {
        this.subscribedCharacteristics++;
        NetworkDevice networkDevice = NetworkDeviceList.Where(d => d.Address.Equals(deviceAddressNotify)).Select(d => d).FirstOrDefault();
        if (this.verbose) StatusMessage = $"Subscribed to {this.subscribedCharacteristics} characteristics so far.";

        // fully connected when subscribed to all except one characteristics (command characteristic is only used for MTU)
        if (networkDevice != null && OnDeviceReady != null && this.subscribedCharacteristics >= Networking.Characteristics.Count - 1)
        {
            StatusMessage = string.Format("Device completely connected to {0}", networkDevice.Name);
            OnDeviceReady(networkDevice);
            _deviceToConnect = null;
            SetState(States.None, 0.1f);
        }
        else if (this.verbose) StatusMessage = $"Device not ready yet. Device {(networkDevice == null ? "NULL" : networkDevice.Name)} for address {deviceAddressNotify}, " +
                $"OnDeviceReady is {(OnDeviceReady == null ? "NULL" : "not null")}, {this.subscribedCharacteristics} subscribed vs. {Networking.Characteristics.Count - 1} " +
                $"required subscribed characteristics.";
    }

    private new void Reset()
    {
        base.Reset();
        this.subscribedCharacteristics = 0;
    }

    public new void StartServer(string networkName, Action<NetworkDevice> onDeviceReady, Action<NetworkDevice> onDeviceDisconnected, Action<NetworkDevice, string, byte[]> onDeviceDataBytes)
    {
        Debug.LogError("Custom Networking: Wrong StartServer method!");
        if (OnError != null) OnError("Custom Networking: Wrong StartServer method!");
    }

    public void StartServer(string networkName, Action<NetworkDevice> onDeviceReady, Action<NetworkDevice> onDeviceDisconnected, Action<NetworkDevice, string, byte[]> onDeviceDataBytes, Action<NetworkDevice, string, string> onDeviceDataString)
    {
        BluetoothLEHardwareInterface.Log(string.Format("server network: {0}", networkName));

        if (!_serverStarted)
        {
            Reset();

            _serverStarted = true;
            _networkName = networkName;

            OnDeviceReady = onDeviceReady;
            OnDeviceDisconnected = onDeviceDisconnected;
            OnDeviceDataBytes = onDeviceDataBytes;
            OnDeviceDataString = onDeviceDataString;

            SetState(States.StartScan, 0.1f);
        }
    }

    public new void WriteDevice(NetworkDevice device, byte[] bytes, Action onWritten)
    {
        StatusMessage = $"Writing to device {device.Name} on sample characteristic ({SampleCharacteristic.CharacteristicUUID})";

        BluetoothLEHardwareInterface.WriteCharacteristic(device.Address, SampleCharacteristic.ServiceUUID, SampleCharacteristic.CharacteristicUUID, bytes, bytes.Length, true, (Characteristic) =>
        {
            if (onWritten != null)
                onWritten();
        });
    }

    public void WriteDeviceString(NetworkDevice device, string data, Action onWritten)
    {
        StatusMessage = $"Writing to device {device.Name} on string characteristic ({StringCharacteristic.CharacteristicUUID}): {data}";

        byte[] bytes = Encoding.ASCII.GetBytes(data);
        if (bytes.Length > _mtu || bytes.Length > this.stringCharacteristicSize) bytes = bytes[..Math.Min(_mtu, this.stringCharacteristicSize)];

        BluetoothLEHardwareInterface.WriteCharacteristic(device.Address, StringCharacteristic.ServiceUUID, StringCharacteristic.CharacteristicUUID, bytes, bytes.Length, true, (Characteristic) =>
        {
            if (onWritten != null)
                onWritten();
        });
    }

    public new void StartClient(string networkName, string clientName, Action onStartedAdvertising, Action<string, string, byte[]> onCharacteristicWritten)
    {
        Debug.LogError("Custom Networking: Wrong StartClient method!");
        if (OnError != null) OnError("Custom Networking: Wrong StartClient method!");
    }

    public void StartClient(string networkName, string clientName, Action onStartedAdvertising, Action<string, string, byte[]> onByteCharacteristicWritten, Action<string, string, string> onStringCharacteristicWritten)
    {
        Reset();

        BluetoothLEHardwareInterface.Log(string.Format("client network: {0}, client name: {1}", networkName, clientName));

        BluetoothLEHardwareInterface.PeripheralName(networkName + ":" + clientName);

        BluetoothLEHardwareInterface.RemoveServices();
        BluetoothLEHardwareInterface.RemoveCharacteristics();

        BluetoothLEHardwareInterface.CBCharacteristicProperties properties =
            BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyRead |
            BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyWrite |
            BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyNotify |
            BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyBroadcast |
            BluetoothLEHardwareInterface.CBCharacteristicProperties.CBCharacteristicPropertyWriteWithoutResponse;

        BluetoothLEHardwareInterface.CBAttributePermissions permissions =
            BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsReadable |
            BluetoothLEHardwareInterface.CBAttributePermissions.CBAttributePermissionsWriteable;

        // has to be handled in one action that then distinguishes by UUID, because the BLEHardwareInterface only saves the last characteristic action
        Action<string, byte[]> action = (characteristic, bytes) =>
        {
            if (this.verbose) StatusMessage = $"Characteristic {characteristic} was written to with {bytes.Length} bytes.";

            if (characteristic.ToUpper().Equals(CommandCharacteristic.CharacteristicUUID))
            {
                if (bytes.Length >= 2 && bytes[0] == 0xA5 && bytes[1] > 0)
                {
                    _mtu = bytes[1];
                    StatusMessage = $"Client MTU set to {_mtu}";
                    if (_mtu < this.stringCharacteristicSize) StatusMessage = $"WARNING: MTU smaller than the string characteristic size (MTU of {_mtu} vs. {this.stringCharacteristicSize})!";
                }
            }
            else if (characteristic.ToUpper().Equals(SampleCharacteristic.CharacteristicUUID))
            {
                if (this.verbose) StatusMessage = $"Characteristic action byte ({characteristic}): {bytes[0]}, {(bytes.Length > 1 ? bytes[1] : "")}";
                if (onByteCharacteristicWritten != null) onByteCharacteristicWritten(clientName, characteristic, bytes);
            }
            else if (characteristic.ToUpper().Equals(StringCharacteristic.CharacteristicUUID))
            {
                if (this.verbose) StatusMessage = $"Characteristic action string ({characteristic}): {bytes[0]}, {(bytes.Length > 1 ? bytes[1] : "")}";
                if (onStringCharacteristicWritten != null) onStringCharacteristicWritten(clientName, characteristic, Encoding.ASCII.GetString(bytes));
            }
        };

        BluetoothLEHardwareInterface.CreateCharacteristic(CommandCharacteristic.CharacteristicUUID, properties, permissions, null, 5, action);
        BluetoothLEHardwareInterface.CreateCharacteristic(SampleCharacteristic.CharacteristicUUID, properties, permissions, null, 5, action);
        BluetoothLEHardwareInterface.CreateCharacteristic(StringCharacteristic.CharacteristicUUID, properties, permissions, null, this.stringCharacteristicSize, action);

        BluetoothLEHardwareInterface.CreateService(SampleCharacteristic.ServiceUUID, true, (characteristic) =>
        {
            if (this.verbose) StatusMessage = $"Created service ({characteristic})";
        });

        BluetoothLEHardwareInterface.StartAdvertising(() =>
        {
            if (onStartedAdvertising != null)
                onStartedAdvertising();

            ClientAdvertising = true;
        });
    }

    public void SendFromClientString(string data)
    {
        if (this.verbose) StatusMessage = "SendFromClientString";

        byte[] bytes = Encoding.ASCII.GetBytes(data);
        if (bytes.Length > _mtu || bytes.Length > this.stringCharacteristicSize) bytes = bytes[..Math.Min(_mtu, this.stringCharacteristicSize)];

        BluetoothLEHardwareInterface.UpdateCharacteristicValue(StringCharacteristic.CharacteristicUUID, bytes, bytes.Length);
    }
}
