/* This is a simple example to show the steps and one possible way of
 * automatically scanning for and connecting to a device to receive
 * notification data from the device.
 */

using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Serialization;

public class StretchSenseAPI : MonoBehaviour
{
    //------------------------------------------------------------//
    //				DEFINITIONS AND DECLARATIONS
    //------------------------------------------------------------//

    //---------- Devicename, Services, Chacteristics ---------------//

    private string DeviceName = "StretchSense";
    private string ServiceUUID = "00001701-7374-7265-7563-6873656e7365";
    private string SensorCharacteristic = "00001702-7374-7265-7563-6873656e7365";
    private string IMUCharacteristic = "00001703-7374-7265-7563-6873656e7365";
    private string FilterCharacteristic = "00001706-7374-7265-7563-6873656e7365"; //Filtering Characteristic

    //---------- StretchSense BLE circuit---------------//

    public Dropdown GloveTypeDropdown;
    public Slider   FilterSlider;

    private bool isLeftGlove;

    private bool isRecording;

    public void setIsLeftGLove(bool isleft)
    {
        isLeftGlove = isleft;
    }

    private SSL_Circuit leftGloveCircuit = null;
    private SSL_Circuit rightGloveCircuit = null;

    //---------- General Variables ---------------//

    public Dictionary<string, bool> _peripheralList;

    public bool initBluetooth = false;

    private SaveFile saveFile;

    public GloveUIManager GloveUiManager = null;

    public Button buttonLeftGlove;

    public Button buttonRightGlove;

    public GameObject StretchSenseController;

//------------------------------------------------------------//
//							RESET
//------------------------------------------------------------//

    private void Reset()
    {
        if (_peripheralList != null) _peripheralList.Clear();
    }

//------------------------------------------------------------//
//							START
//------------------------------------------------------------//

    public void StartProcess()
    {
        if (GloveUiManager == null) GloveUiManager = StretchSenseController.GetComponent<GloveUIManager>();

        if (!initBluetooth)
            BluetoothLEHardwareInterface.Initialize(true, false, () => { initBluetooth = true; }, (error) => { });

        // start scanning after 1 second 
        Invoke("scan", 1f);
    }

    public SSL_Circuit getLeftGloveCircuit()
    {
        return leftGloveCircuit;
    }

    public SSL_Circuit getRightGloveCircuit()
    {
        return rightGloveCircuit;
    }

    // Use this for initialization
    private void Start()
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
    private bool showTutorial = true;
    private string discoveredDeviceAddress = "";
    private string discoveredDeviceName = "";

    private void ShowTutorialWindow(int windowID)
    {
        var myButtonStyle = new GUIStyle(GUI.skin.button);
        myButtonStyle.fontSize = 25;
        GUI.Label(new Rect(20, 15, 700, 275),
            "1. Please turn on Bluetooth on the respective left/right gloves\n" + "& on your device. \n" +
            "2. Please use left/right power button on the bottom panel to start\n" +
            "scanning and connecting respective glove.\n" +
            "3. You can use switch button on bottom panel to swap left &\n" + "right glove. \n" +
            "4. Share & record button allows you to record\n" + "sensor data & share. ", myButtonStyle);

        var actionButtonStyle = new GUIStyle(GUI.skin.button);
        actionButtonStyle.fontSize = 25;

        if (GUI.Button(new Rect(300, 300, 150, 60), "Ok", actionButtonStyle)) showTutorial = false;
    }

    private void ShowGUI(int windowID)
    {
        // You may put a label to show a message to the player
        var myButtonStyle = new GUIStyle(GUI.skin.button);
        myButtonStyle.fontSize = 25;

        GUI.Label(new Rect(25, 15, 500, 150),
            "Do you wish to connect to \n" + discoveredDeviceName + "\n" + discoveredDeviceAddress, myButtonStyle);

        // You may put a button to close the pop up too

        var actionButtonStyle = new GUIStyle(GUI.skin.button);
        actionButtonStyle.fontSize = 25;

        if (GUI.Button(new Rect(350, 200, 150, 60), "Ok", actionButtonStyle))
        {
            showPopUp = false;
            connectBluetooth(discoveredDeviceAddress);
            //Connect to this device
        }

        if (GUI.Button(new Rect(50, 200, 150, 60), "No", actionButtonStyle)) showPopUp = false;
    }

    private void OnGUI()
    {
        if (showTutorial)
        {
            var myButtonStyle = new GUIStyle(GUI.skin.button);
            myButtonStyle.fontSize = 25;

            GUI.Window(1, new Rect(Screen.width / 2 - 300, Screen.height / 2 - 75, 740, 400), ShowTutorialWindow, "",
                myButtonStyle);
        }

        if (showPopUp)
        {
            var myButtonStyle = new GUIStyle(GUI.skin.button);
            myButtonStyle.fontSize = 25;

            GUI.Window(0, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 75, 550, 300), ShowGUI,
                "Device Discovered", myButtonStyle);
        }
    }

    public void SliderChange()
    {

        //Set filter
        int Filter = (int) FilterSlider.value;
        //Apply filter
        var filter = (byte)Filter;

        if (leftGloveCircuit != null) 
            SendByte(leftGloveCircuit.get_uuid(), ServiceUUID, FilterCharacteristic, filter);

        if (rightGloveCircuit != null)
            SendByte(rightGloveCircuit.get_uuid(), ServiceUUID, FilterCharacteristic, filter);

    }



    /////////////////////////////////////////////////////////////////

    //------------------------------------------------------------//
    //						BLE FUNCTIONS
    //------------------------------------------------------------//

    //---------- Scaning for BLE Devices ----------------//

    public void scan()
    {
        // the first callback will only get called the first time this device is seen 
        // this is because it gets added to a list in the BluetoothDeviceScript 
        // after that only the second callback will get called and only if there is 
        // advertising data available 
        BluetoothLEHardwareInterface.Log("SslAPI - Starting scan");
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null,
            (address, name) => { AddPeripheral(name, address); },
            (address, name, rssi, advertisingInfo) => { AddPeripheral(name, address); }, false, false);
    }

    //---------- Converting the Sensor data into capacitances ----------------//

    public void SSLNotificationReceived(string address, string characteristic, byte[] dataBytes)
    {
        if (dataBytes != null)
        {
            //Notificaiton recieved update capacitance values
            var array_capacitance_int = Utility.RandomUtility.convertBytetoArray(dataBytes);

            var capacitance = new float[10];

            for (var index = 0; index < 10; index++) capacitance[index] = array_capacitance_int[index] / 10;

            var name = "StretchSense";

            if (leftGloveCircuit != null && leftGloveCircuit.get_uuid() == address)
            {
                name = "Left Glove";
                leftGloveCircuit.onNotificationUpdate(array_capacitance_int); //Update SSL circuit with new data
            }

            if (rightGloveCircuit != null && rightGloveCircuit.get_uuid() == address)
            {
                name = "Right Glove";
                rightGloveCircuit.onNotificationUpdate(array_capacitance_int); //Update SSL circuit with new data
            }

            if (saveFile != null && isRecording) saveFile.record(name, capacitance);
        }
    }

    //---------- Adding available BLE Devices to a List ----------------//

    private void AddPeripheral(string name, string address)
    {
        BluetoothLEHardwareInterface.Log("Found: " + name + " Address: " + address);

        //Device discovered that matches requirements
        if (_peripheralList == null) _peripheralList = new Dictionary<string, bool>();

        if (!_peripheralList.ContainsKey(address) || (_peripheralList.ContainsKey(address) && !_peripheralList[address]))
        {
            if (name == DeviceName)
            {
                BluetoothLEHardwareInterface.Log("SslAPI - AddPeripheral to _peripheralList: " + name + " " + address);

                _peripheralList[address] = false;

                //Stop scanning and connect to this device

                if (!showPopUp && !_peripheralList[address])
                {
                    discoveredDeviceName = name;
                    discoveredDeviceAddress = address;
                    showPopUp = true;
                    BluetoothLEHardwareInterface.StopScan(); //Todo check if this works??
                }
            }
        }
        else
        {
            BluetoothLEHardwareInterface.Log("SslAPI - No address found");
        }
    }

//---------- connecting/disconnecting via BLE ----------------//

    public void connectBluetooth(string addr)
    {
        BluetoothLEHardwareInterface.ConnectToPeripheral(addr, (address) => { }, (address, serviceUUID) => { },
            (address, serviceUUID, characteristicUUID) =>
            {
                // this will get called when the device connects 
                BluetoothLEHardwareInterface.StopScan();
                //Update connected control circuit

                isLeftFilterSet = false;
                isRightFilterSet = false;

                int GloveType = TrainingData.GLOVES_5;  //Default Glove 7ch
                switch (GloveTypeDropdown.value) {

                    case 0:
                        GloveType = TrainingData.GLOVES_5;
                        break;

                    case 1:
                        GloveType = TrainingData.GLOVES_7;
                        break;

                    case 2:
                        GloveType = TrainingData.GLOVES_10;
                        break;
                }
                

                if (isLeftGlove)
                {
                    BluetoothLEHardwareInterface.Log("Found Left Glove: " + address);
                    leftGloveCircuit = new SSL_Circuit(GloveType);
                    leftGloveCircuit.set_uuid(address);
                    GloveUiManager.setTurnOnImage(buttonLeftGlove);
                }
                else
                {
                    BluetoothLEHardwareInterface.Log("Found Right Glove: " + address);
                    rightGloveCircuit = new SSL_Circuit(GloveType);
                    rightGloveCircuit.set_uuid(address);
                    GloveUiManager.setTurnOnImage(buttonRightGlove);
                }

                _peripheralList[address] = true;
                subscribeToCharacteristic(address, ServiceUUID,
                    SensorCharacteristic); //Enable notifications on sensing characteristic
                //subscribeToCharacteristic(address, ServiceUUID, IMUCharacteristic);    //Enable notifications on sensing characteristic
            }, (address) =>
            {
                // Device on disconnect

                if (leftGloveCircuit != null && leftGloveCircuit.get_uuid() == address)
                {
                    BluetoothLEHardwareInterface.Log("SslAPI - Disconnect left device : " + address);
                    leftGloveCircuit = null;

                    GloveUiManager.setTurnOffImage(buttonLeftGlove);
                }

                if (rightGloveCircuit != null && rightGloveCircuit.get_uuid() == address)
                {
                    BluetoothLEHardwareInterface.Log("SslAPI - Disconnect right device : " + address);

                    rightGloveCircuit = null;
                    GloveUiManager.setTurnOffImage(buttonRightGlove);
                }

                _peripheralList.Remove(address);

                // start scanning after 1 second 
//				Invoke ("scan", 1f);			
            });
    }

    //---------- Subscribing to Characteristic ----------------//

    Boolean isLeftFilterSet  = false;
    Boolean isRightFilterSet = false;

    private void subscribeToCharacteristic(string address, string serviceUUID, string characteristicUUID)
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristic(address, serviceUUID, characteristicUUID, null,
            (characteristic, bytes) =>
            {
                if (bytes.Length == 0)
                {
                    // do nothing 
                }
                else
                {
                    if (leftGloveCircuit != null && address == leftGloveCircuit.get_uuid())
                    {
                        if (!isLeftFilterSet)
                        {
                            var filter = (byte) ((int)FilterSlider.value);//leftGloveCircuit.getFilter();
                            SendByte(address, ServiceUUID, FilterCharacteristic, filter);
                            isLeftFilterSet = true;
                        }
                    }
                    else if (rightGloveCircuit != null && address == rightGloveCircuit.get_uuid())
                    {
                        if (!isRightFilterSet)
                        {
                            var filter = (byte)((int)FilterSlider.value);//rightGloveCircuit.getFilter();
                            SendByte(address, ServiceUUID, FilterCharacteristic, filter);
                            isRightFilterSet = true;
                        }
                    }

                    // convert and store the notification into a StretchSense circuit object
                    SSLNotificationReceived(address, characteristicUUID, bytes);
                }
            });
    }

    private void disconnect(string name)
    {
        BluetoothLEHardwareInterface.DisconnectPeripheral(name, null);
    }

//------------- Writing to Characteristic -------------------//	
    private void SendByte(string address, string serviceUUID, string characteristicUUID, byte value)
    {
        BluetoothLEHardwareInterface.Log("SslAPI - SendByte()");
        var data = new byte[] {value};
        BluetoothLEHardwareInterface.WriteCharacteristic(address, serviceUUID, characteristicUUID, data, data.Length,
            true, (characteristic) => { BluetoothLEHardwareInterface.Log("SslAPI - Write Succeeded " + address); });
    }

    public void disconnectLeft()
    {
        if (leftGloveCircuit != null)
        {
            disconnect(leftGloveCircuit.get_uuid());
            leftGloveCircuit = null;
        }
        
        BluetoothLEHardwareInterface.StopScan();

    }

    public void disconnectRight()
    {
        if (rightGloveCircuit != null)
        {
            disconnect(rightGloveCircuit.get_uuid());
            rightGloveCircuit = null;
        }
        
        BluetoothLEHardwareInterface.StopScan();

    }

    public void recordData()
    {
        if (saveFile == null) saveFile = new SaveFile();

        saveFile.OpenFile();
        isRecording = true;
    }

    public void stopRecordData()
    {
        isRecording = false;
        saveFile.CloseFile();
    }

    public string getSensorFilePath()
    {
        BluetoothLEHardwareInterface.Log(" Sharing file : " + saveFile.sensordatafilepath);

        return saveFile.sensordatafilepath;
    }

    public void swapGLove()
    {
        var tempGLove = leftGloveCircuit;
        leftGloveCircuit = rightGloveCircuit;
        rightGloveCircuit = tempGLove;

        if (leftGloveCircuit != null)
            GloveUiManager.setTurnOnImage(buttonLeftGlove);
        else
            GloveUiManager.setTurnOffImage(buttonLeftGlove);

        if (rightGloveCircuit != null)
            GloveUiManager.setTurnOnImage(buttonRightGlove);
        else
            GloveUiManager.setTurnOffImage(buttonRightGlove);
    }
}