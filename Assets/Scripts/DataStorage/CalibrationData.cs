using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationData {
   //Generic data structure for saving and loading    
   public float[] Calibration { get; set; }
    
    public CalibrationData mCalibrationData { get; private set; }
    public float x;
    public float y;
    public float [] test;
      
    public bool isCalibrationComplete() {
        //Scan through all items and check for non-zero values
        for (int i = 0; i < Calibration.Length; i++) {

            if (Calibration[i] != 0) {
                return true;
            }

        }
        //All values in calibration are 0
        return false;
    }
    
    private void OnEnable() {
        //Load data from preferences
        mCalibrationData = CalibrationStorage.LoadCalibration();
        //Check if any calibration has been loaded
        if (mCalibrationData.isCalibrationComplete())
        {
            //Load and use calibration data
            test = mCalibrationData.Calibration;
            x = test[0];
            y = test[1];

//            transform.Rotate(x, y, 0);
        }
        else {
            //Run calibration    

        }

    }



}
