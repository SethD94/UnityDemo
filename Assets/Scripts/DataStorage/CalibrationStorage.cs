using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CalibrationStorage{

    public static void SaveCalibration(float[] CalibrationSet) {

        //Convert to string array
        string[] ArrayStr = new string[CalibrationSet.Length];
        for (int i = 0; i < CalibrationSet.Length; i++) {
            ArrayStr[i] = CalibrationSet[i].ToString();
        }

        //Comma seperate array items and create string
        string s = string.Join(",", ArrayStr);

        //Save string to preferences
        PlayerPrefs.SetString("Calibration", s);
    }

    public static CalibrationData LoadCalibration() {
        //Load string from memory
        string s = PlayerPrefs.GetString("Calibration");
        //Conver to string []
        string[] sArray = s.Split(',');
        //Create float array size of string []
        float[] Calibration = new float[sArray.Length];
        
        //Populate float array
        for (int i = 0; i < sArray.Length; i++) {
            Calibration[i] = float.Parse(sArray[i]);
        }

        CalibrationData mData = new CalibrationData();
        mData.Calibration = Calibration;
        return mData;
    }

    public static void ClearCalibration() {
        //Set all saved values to zero (Set calbration complete to zero)
        PlayerPrefs.SetString("Calibration", "0");
    }
}
