using UnityEngine;
using System;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

public class SSL_Circuit
{

    //----- Initialisation ----//
    private string uuid;
    private int nChannel = 10;
    private int[] array_capacitance_raw = new int[10];
    private int filter = 20;

    private TrainingData mTraining = new TrainingData(1);

    public SSL_Circuit(int DemostrationType) {
        mTraining = new TrainingData(DemostrationType);
    }


    //Notification handler
    public void onNotificationUpdate(int[] array_capacitance_int)
    {
        //Load data to array capacitance raw
        array_capacitance_raw = array_capacitance_int;
        //Update training algorthm
        mTraining.Update(array_capacitance_raw);
    }

    //Get/Set Variables
    public int[] getCap()
    {
        return array_capacitance_raw;
    }

    public int getFilter()
    {

        return filter;
    }

    public void set_uuid(string new_uuid)
    {
        uuid = new_uuid;
    }

    public string get_uuid()
    {
        return uuid;
    }


    //Calibration
    public Boolean isCalibrating()
    {
        return !mTraining.isComplete();
    }

    public void startCalibrating()
    {
        mTraining.setComplete(false);
    }

    public double[] getCalibratedData()
    {

        int[] DCbiased = new int[array_capacitance_raw.Length + 1];
        DCbiased[0] = 1;

        for (int i = 0; i < array_capacitance_raw.Length; i++)
        {
            DCbiased[i + 1] = array_capacitance_raw[i];
        }

        //Retrieve weightings (theta) from training algo and multiply against current capacitance data
        double[] Position = mTraining.ApplyTransformation(DCbiased, mTraining.getTheta());
        //Set range limits
        for (int i = 0; i < Position.Length; i++)
        {
            if (Position[i] < mTraining.TrainingYMin[i])
            {
                Position[i] = mTraining.TrainingYMin[i];
            }
            else if (Position[i] > mTraining.TrainingYMax[i])
            {
                Position[i] = mTraining.TrainingYMax[i];
            }
        }

        return Position;

    }

    public void restoreCalibration()
    {
        Vector<double>[] trainingData = null;
        String key = "kneeCalibration";

        BluetoothLEHardwareInterface.Log(" restoring calibration : " + key);

        if (PlayerPrefs.GetInt(key) > 0)
        {
            int vectorLength = PlayerPrefs.GetInt(key);

            trainingData = new Vector<double>[vectorLength];

            for (int index = 0; index < vectorLength; index++)
            {
                float[] tempData = PlayerPrefsX.GetFloatArray(key + "" + index);

                double[] realData = Array.ConvertAll(tempData, x => (double)x);
                Vector<double> v = Vector<double>.Build.Dense(realData.Length);

                for (int i = 0; i < realData.Length; i++)
                {
                    v[i] = realData[i];
                }

                trainingData[index] = v;
            }
        }


        if (trainingData != null)
        {
            mTraining.setTheta(trainingData);

        }
    }

    public double[] getTrainingTarget()
    {
        return mTraining.getCurrentPosition();
    }

    public void RecordPosition()
    {
        mTraining.RecordNow();
    }


}
