/* This is a simple example to show the steps and one possible way of
 * automatically scanning for and connecting to a device to receive
 * notification data from the device.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSLBleAPI : MonoBehaviour
{

    //------------------------------------------------------------//
    //				DEFINITIONS AND DECLARATIONS
    //------------------------------------------------------------//


    //---------- Devicename, Services, Chacteristics ---------------//

    private string DeviceName           = "StretchSense";
    private string ServiceUUID          = "00001701-7374-7265-7563-6873656e7365";
    private string SensorCharacteristic = "00001702-7374-7265-7563-6873656e7365";
    private string FilterCharacteristic = "00001706-7374-7265-7563-6873656e7365"; //Filtering Characteristic
    private Boolean setfilter = false;

    //---------- StretchSense BLE circuit---------------//

    private SSL_Circuit controllerCircuit = null;

    //---------- General Variables ---------------//

    private bool isConnected = false;
    public Dictionary<string, bool> _peripheralList;
    public Slider FilterSlider;

    //------------------------------------------------------------//
    //							RESET
    //------------------------------------------------------------//

    public bool initBluetooth = false;

    void Reset()
    {
        isConnected = false;

        if (_peripheralList != null)
        {
            _peripheralList.Clear();
        }
    }


    //------------------------------------------------------------//
    //							START
    //------------------------------------------------------------//


    public void StartProcess()
    {
        if (!initBluetooth)
            BluetoothLEHardwareInterface.Initialize(true, false, () => { initBluetooth = true; }, (error) => { });

        // start scanning after 1 second 
        Invoke("scanDevices", 1f);
    }

    //Return object that stores capacitance data/ratio data
    public SSL_Circuit getCircuit()
    {

        return controllerCircuit;
    }


    // Use this for initialization
    void Start()
    {

        BluetoothLEHardwareInterface.DeInitialize(() =>
        {

        });
        FilterSlider.onValueChanged.AddListener(delegate { SliderChange(); });
    }



    /////////////////////////////////////////////////////////////////
    //------------------------------------------------------------//
    //						UI FUNCTIONS
    //------------------------------------------------------------//

    private bool showPopUp = false;
    private string discoveredDeviceAddress = "";
    private string discoveredDeviceName = "";

    void ShowGUI(int windowID)
    {
        // You may put a label to show a message to the player

        GUI.Label(new Rect(65, 65, 200, 90), "Do you wish to connect to \n" + discoveredDeviceName + "\n" + discoveredDeviceAddress);

        // You may put a button to close the pop up too

        if (GUI.Button(new Rect(150, 150, 75, 30), "Ok"))
        {
            showPopUp = false;
            connectBluetooth(discoveredDeviceAddress);
            //Connect to this device
        }
        if (GUI.Button(new Rect(50, 150, 75, 30), "No"))
        {
            showPopUp = false;
            //Do not connect to this device
        }

    }


    void OnGUI()
    {
        if (showPopUp)
        {
            GUI.Window(0, new Rect((Screen.width / 2) - 150, (Screen.height / 2) - 75
                   , 300, 250), ShowGUI, "Device Discovered");
        }

    }

    public void SliderChange()
    {

        //Set filter
        int Filter = (int)FilterSlider.value;
        //Apply filter
        var filter = (byte)Filter;

        if (controllerCircuit != null)
            SendByte(controllerCircuit.get_uuid(), ServiceUUID, FilterCharacteristic, filter);

    }

    /////////////////////////////////////////////////////////////////


    //------------------------------------------------------------//
    //						BLE FUNCTIONS
    //------------------------------------------------------------//


    //---------- Scaning for BLE Devices ----------------//

    public void scanDevices()
    {
        // the first callback will only get called the first time this device is seen 
        // this is because it gets added to a list in the BluetoothDeviceScript 
        // after that only the second callback will get called and only if there is 
        // advertising data available 

        BluetoothLEHardwareInterface.Log("SslAPI - Starting scan");
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null,
            (address, name) => { AddKneePeripheral(name, address); },
            (address, name, rssi, advertisingInfo) => { AddKneePeripheral(name, address); }, false, false);


    }


    //---------- Converting the Sensor data into capacitances ----------------//

    public void SleeveNotificationReceived(string address, byte[] dataBytes)
    {

        if (dataBytes != null)
        {

            int[] array_capacitance_int = Utility.RandomUtility.convertBytetoArray(dataBytes);

            if (controllerCircuit.get_uuid() == address)
            {
                controllerCircuit.onNotificationUpdate(array_capacitance_int); //Update SSL circuit with new data
            }

        }
    }

    //---------- Adding available BLE Devices to a List ----------------//

    void AddKneePeripheral(string name, string address)
    {
        //Device discovered that matches requirements
        BluetoothLEHardwareInterface.Log("KneeBleAPI Got Device: " + name + " " + address);

        if (_peripheralList == null)
        {
            _peripheralList = new Dictionary<string, bool>();
        }

        if (!_peripheralList.ContainsKey(address) ||
            (_peripheralList.ContainsKey(address) && !_peripheralList[address]))
        {
            if (name == DeviceName)
            {
                BluetoothLEHardwareInterface.Log("KneeBleAPI - AddPeripheral to _peripheralList: " + name + " " +
                                                 address + " " + isConnected + " " + showPopUp);

                _peripheralList[address] = false;

                //Stop scanning and connect to this device
                BluetoothLEHardwareInterface.StopScan(); //Todo check if this works??

                if (!showPopUp && !isConnected)
                {
                    discoveredDeviceName = name;
                    discoveredDeviceAddress = address;
                    showPopUp = true;
                }
            }
        }

        else
        {
            BluetoothLEHardwareInterface.Log("KneeBleAPI - No address found");
        }
    }

    public int Datatype;


    //---------- connecting/disconnecting via BLE ----------------//

    public void connectBluetooth(string addr)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(
            addr,
            (address) => { },
            (address, serviceUUID) => { },
            (address, serviceUUID, characteristicUUID) => {
                // this will get called when the device connects 
                isConnected = true;
                BluetoothLEHardwareInterface.StopScan();
                //Update connected control circuit
                controllerCircuit = new SSL_Circuit(Datatype);
                controllerCircuit.set_uuid(address);
                byte filter = (byte)((int)FilterSlider.value); //(byte)controllerCircuit.getFilter();
                setfilter = false;
                subscribeToCharacteristics(address, ServiceUUID, SensorCharacteristic);    //Enable notifications on sensing characteristic
            },
            (address) => {
                // Device on disconnect
                isConnected = false;

                _peripheralList.Remove(address);

                Reset();
            });
    }

    //---------- Subscribing to Characteristic ----------------//

    void subscribeToCharacteristics(string address, string serviceUUID, string characteristicUUID)
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristic(address, serviceUUID, characteristicUUID, null, (characteristic, bytes) => {
            if (!setfilter)
            {
                setfilter = true;
                byte filter = (byte)controllerCircuit.getFilter();
                SendByte(address, ServiceUUID, FilterCharacteristic, filter);
            }

            if (bytes.Length == 0)
            {
                // do nothing 
            }
            else
            {
                // convert and store the notification into a StretchSense circuit object
                SleeveNotificationReceived(address, bytes);
            }
        });
    }


    //------------- Writing to Characteristic -------------------//	
    void SendByte(string address, string serviceUUID, string characteristicUUID, byte value)
    {
        BluetoothLEHardwareInterface.Log("SslAPI - SendByte()");
        byte[] data = new byte[] { value };
        BluetoothLEHardwareInterface.WriteCharacteristic(address, serviceUUID, characteristicUUID, data, data.Length, true, (characteristic) => {

            BluetoothLEHardwareInterface.Log("SslAPI - Write Succeeded");
        });
    }

    public void setFilterCharacteristic(byte value)
    {
        BluetoothLEHardwareInterface.Log("SslAPI - SendByte()");
        byte[] data = new byte[] { value };
        BluetoothLEHardwareInterface.WriteCharacteristic(discoveredDeviceAddress, ServiceUUID, FilterCharacteristic, data, data.Length, true, (characteristic) => {

            BluetoothLEHardwareInterface.Log("SslAPI - Write Succeeded");
        });
    }

    public void disconnect()
    {
        if (controllerCircuit != null)
        {
            BluetoothLEHardwareInterface.DisconnectPeripheral(controllerCircuit.get_uuid(), null);
            _peripheralList.Remove(controllerCircuit.get_uuid());

            BluetoothLEHardwareInterface.StopScan();
            controllerCircuit = null;
        }
    }

}
