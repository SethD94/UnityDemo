using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SaveFile {

    //--Example usecase----------------------------------------------------------------------------------------------------------------//

    StreamWriter sr;
    float StartTime = 0;
    
    public string sensordatafilepath { get; set; }

    //DemoPurpose
    void Start()
    {
        OpenFile();
        
    }

    public void record(String deviceName, float[] data) {
        //Create timestamp relative to first recording (In this example it starts on app start, it could be inserted within check for connected device)
        if (StartTime == 0) {
            StartTime = Time.time * 1000;
        }
        int mTime = (int)Math.Round(Time.time * 1000 - StartTime);
        SaveToFile(deviceName, mTime, data);  //Write timestamp and cap values to file
    }

    void OnApplicationQuit() {
        CloseFile();
    }

    void OnApplicationPause() {
        sr.Flush();
    }



    //--Function Calls-------------------------------------------------------------------------------------------------------------------//




    //Call on Update
    public void SaveToFile(String deviceName, int timestamp_ms, float[] capValue)
    {
        //Convert to string array

        if (deviceName == null)
        {
            deviceName = "StretchSense";
        }
        
        sr.Write(deviceName);

        sr.Write("{0},", timestamp_ms);
        for (int i = 0; i < 10; i++) {
            sr.Write("{0},", capValue[i]);
        }
        sr.WriteLine();
        

    }

    //Call onStart()
    public void OpenFile()
    {
        //FileCount variable
        int count = 0;

        //Determine file path to write to
        string filePath = Application.persistentDataPath + "/LogFile"+count+".csv";
        //Check file name is unique
        while (File.Exists(filePath)) {
            count++;
            filePath = Application.persistentDataPath + "/LogFile" + count + ".csv";
        }

        //Create file
        sr = File.CreateText(filePath);

        sensordatafilepath = filePath;
        //Write header to file
        sr.WriteLine("Glove, Time (ms),Sensor 1(pF),Sensor 2(pF),Sensor 3(pF),Sensor 4(pF),Sensor 5(pF),Sensor 6(pF),Sensor 7(pF),Sensor 8(pF),Sensor 9(pF),Sensor 10(pF)");
    }

    //Call onDestroy()
    public void CloseFile()
    {
        sr.Close();
    }

}
