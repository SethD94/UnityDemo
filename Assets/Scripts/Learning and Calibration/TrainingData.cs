using System;
using MathNet.Numerics.LinearAlgebra;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Complex;
using UnityEngine;


public class TrainingData
{

    private int TrainingIndex = 0;

    private bool CaptureCalibrationData = false;
    private bool FirstPress = true;


    private int samplesIndex = 0;
    private static int samplesPerPosition = 50;

    private bool complete = true;

    private int nOutputs;  //I.e. Right Hip, Right Knee, Left Hip, Left Kneey

    private int TrainingPositions;
    //X     Y      Z//         
    private double[,] TrainingY;
    private int[][] ActiveSensor;
    int[] All = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    public double[] TrainingYMax;
    public double[] TrainingYMin;
    private double[] currentY;

    private static int nSamples;

    //Results - to pass to regression fit eq
    private double[,] X;
    private double[,] y;

    private static int nSensors = 10;  //I.e. number of sensor inputs (10 stretchsensors + 9 axis IMU = 19)
    private int index = 0;

    //Prevent repeated data effecting results
    private int[] LastX = new int[nSensors];

    //Body part
    public static int GLOVES_10 = 0;    //Splay sensors included
    public static int SLEEVE    = 1;    //Two arm sleeve
    public static int LEGS      = 2;    //Two leg pants
    public static int GLOVES_7  = 3;    //Thumb rotation glove
    public static int GLOVES_5  = 4;    //Basic glove

    public TrainingData(int BodyPart) {

        switch (BodyPart) {

            case 0: //Gloves 10 Sensor
                TrainingY = new double[,] {
                   
                    //Thumb1ABC Thumb2,Index1,Middle1,Ring1,Pinky1,Index2,Middle2,Ring2,Pinky2,Index3,Middle3,Ring3,Pinky3
                    {   0,  0,    0,        -15,    0,    0,    0,    0,    -90,    -90,    -90,    -90,        4,-8,-17,-26},        //Finger straigh bend at knuckle
                    {   0,  0,    0,        -15,    -90,    -90,    -90,    -90, 0,    0,    0,    0,           4,-8,-17,-26},            //Bend finger after knuckle
                    {   0,  0,    0,        -15,    -90,    -90,    -90,    -90, -90,    -90,    -90,    -90,   4,-8,-17,-26},    //Close fist
                    {   0, -15,    -60,        0,   -90,    -90,    -90,    -90, -90,    -90,    -90,    -90,   4,-8,-17,-26},  //Thumb up
                    {   0, -15,    -60,        0,   0,      0,      0,      0,  0,      0,      0,      0,      4,-8,-17,-26},   //Open hand
                    {   0, -15,    0,       40,     0,      0,      0,      0,   0,      0,      0,      0,     4,-8,-17,-26},   //Tuck thumb
                    {   0,  -15,   0,       40,     -80,    -80,    -80,    -80, -80,    -80,    -80,    -80,   4,-8,-17,-26},  //Tuck thumb in fist
                    {   0,  -15,   0,       40,     0,      -80,    -80,    -80,  0,      -80,    -80,    -80,  4,-8,-17,-26},  //Point index
                    {   0,  -15,   0,       40,     0,      0,      -80,    -80, 0,      0,      -80,    -80,   4,-8,-17,-26},  //Point index & middle
                    {   0, 100,  -65,       30,   -55,     0,      0,      0,   -55,     0,      0,      0,     4,-8,-17,-26},     //ok sign
                    {   0, 120,  -70,       15,    0,      -60,    0,      0,   0,      -60,    0,      0,      4,-8,-17,-26},     //touch middle finger
                    {   0, 120,  -70,       15,   0,      -60,    -60,    0, 0,      -60,    -60,    0,         4,-8,-17,-26},     //touch middle finger and ring finger
                    {   0, -15,  -60,       0,      0,      0,      0,      0, 0,      0,      0,      0,       4,-8,-17,-26},        //Spread hand open (target thumb)
                    {   0, 30,  -60,       0,       0,      0,      0,      0,   0,      0,      0,      0,     4,-8,-17,-26},        //Rotate thumb hand open (target thumb)
                    {   0, 60,  -60,       0,       0,      0,      0,      0,   0,      0,      0,      0,     4,-8,-17,-26},        //Rotate thumb hand open (target thumb)
                    {   0, 60,  -45,       0,       0,      0,      0,      0,  0,      0,      0,      0,      4,-8,-17,-26},        //Rotate thumb hand open (target thumb)
                    {   0, 15,  -15,       0,       0,      0,      0,      0,  0,      0,      0,      0,      4,-8,-17,-26},        //Rotate thumb hand open (target thumb)
                    {   0, 0,   0,         0,       0,      0,      0,      0, 0,      0,      0,      0,       4,-8,-17,-26},        //Rotate thumb hand open (target thumb)
                    {   -15, 90,   -45,       0,    -90,    -90,    -90,    -90, -90,    -90,    -90,    -90,   4,-8,-17,-26},        //Thumb over fist
                    {   0, -15,    -60,        0,   0,      0,      0,      0,   0,      0,      0,      0,     -31,  -10,  6,  29},      //Spread hand
                    {   0, -15,    -60,        0,   0,      0,      0,      0,   0,      0,      0,      0,     4,  -8,  -17,  29},        //Seperate Pinky from fingers
                    {   0, -15,    -60,        0,   0,      0,      0,      0,   0,      0,      0,      0,     -32,  4,  0,  -9},         //Seperate Index from fingers
                    {   0, -15,    -60,        0,   0,      0,      0,      0,   0,      0,      0,      0,     -15,  -26,  0,-8},   //Spread index and middle
                    
                };

                //10 Sensor input from circuit, which are relavent
                int[] Thumb  = { 1, 0, 0, 0, 0, 1, 1, 0, 0, 0 }; 
                int[] Index  = { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
                int[] Middle = { 0, 0, 1, 0, 0, 0, 0, 0, 1,  1 };
                int[] Ring   = { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
                int[] Pinky  = { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 };

                //Define which sensors to use during training      
                ActiveSensor = new int[][] {
                    Thumb,Thumb,Thumb,Thumb,Index,Middle,Ring,Pinky,Index,Middle,Ring,Pinky,Index,Middle,Ring,Pinky
                };

                break;
            case 1: //Sleeve
                TrainingY = new double[,]
                {
                    //Right Arm
                    {0, 0, -85, 0, 0, 0, 0, 0}, //Bring Arm over chest
                    {0, 0, -85, 0, 0, 0, 0, 45},
                    {0, 0, -85, 0, 0, 0, 0, 90},
                    {0, 0, -85, 0, 0, 0, 0, 135},
                    {0, 0, -85, 0, 0, 45, 0, 0}, //Bend Elbow
                    {0, 0, -85, 0, 0, 90, 0, 0},
                    {0, 0, -85, 0, 0, 135, 0, 0},
                    {0, 0, -85, 0, 0, 0, -45, 0}, //Raise Arm
                    {0, 0, -85, 0, 0, 0, -90, 0},
                    {0, 0, -85, 0, 0, 0, 70, 0}, //Lower Arm
                    {0, 0, -85, 0, 0, 0, 45, 0},
                    //Left Arm
                    {0, 0, 0, 0, 0, 0, 85, 0},
                    {0, 0, 0, -45, 0, 0, 85, 0},
                    {0, 0, 0, -90, 0, 0, 85, 0},
                    {0, 0, 0, -135, 0, 0, 85, 0},
                    {0, -45, 0, 0, 0, 0, 85, 0},
                    {0, -90, 0, 0, 0, 0, 85, 0},
                    {0, -135, 0, 0, 0, 0, 85, 0},
                    {0, 0, 45, 0, 0, 0, 85, 0},
                    {0, 0, 90, 0, 0, 0, 85, 0},
                    {0, 0, -45, 0, 0, 0, 85, 0},
                    {0, 0, -70, 0, 0, 0, 85, 0},
                };


                //Define which sensors to use during training      
                ActiveSensor = new int[][] {
                    All,All,All,All,All,All,All,All
                };

                break;
            case 2: //Legs

                TrainingY = new double[,] {

                   //Right leg train
                   {0, 0, 0 , 0,       0, 0, 0, 0}, //Standing
                   {0, 0, 0 , 90,      0, 0, 0, 0}, //Knee at 90
                   {40, 0, 0 , 120,    0, 0, 0, 0}, //Hold knee behind 
                   {-90, 0, 0 , 90,    0, 0, 0, 0}, //Knee out straight ahead
                   {-120, 0, 0 , 130,  0, 0, 0, 0}, //Hold knee infront

                   //Left leg train
                   {0, 0, 0, 0,     0,    0, 0, 90}, //Knee at 90
                   {0, 0, 0, 0,     40,   0, 0, 120}, //Hold knee behind 
                   {0, 0, 0, 0,     -90,  0, 0, 90 }, //Knee out straight ahead
                   {0, 0, 0, 0,     -120, 0, 0, 130} //Hold knee infront
        
                };

                //Define which sensors to use during training      
                ActiveSensor = new int[][] {
                    All,All,All,All,All,All,All,All
                };
                break;

            case 3: //Gloves 7 Sensors
                TrainingY = new double[,] {
                        { 0, 0, 0, -15, -90, -90, -90, -90}, //Close fist
                       { 0, -15, -60, 0, -90, -90, -90, -90}, //Thumb up
                       { 0, -15, -60, 0, -90, -90, -90, 0}, //Thumb up, pinky out
                       { 0, -15, -60, 0, 0, 0, 0, 0}, //Open hand
                       { 0, -15, 0, 40, 0, 0, 0, 0}, //Tuck thumb
                       { 0, -15, 0, 40, -80, -80, -80, -80}, //Tuck thumb in fist
                       { 0, -15, 0, 40, 0, -80, -80, -80}, //Point index
                       { 0, -15, 0, 40, 0, 0, -80, -80}, //Point index & middle
                       { -35, 55, -65, 30, -55, 0, 0, 0}, //ok sign
                       { -60, 33, -35, 30, 0, -60, 0, 0}, //touch middle finger
                       { -60, 33, -35, 30, 0, -60, -60, 0}, //touch middle finger and ring finger
                       { 0, -15, -60, 0, 0, 0, 0, 0}, //Spread hand open (target thumb)
                       { 0, 30, -60, 0, 0, 0, 0, 0}, //Rotate thumb hand open (target thumb)
                       { 0, 60, -60, 0, 0, 0, 0, 0}, //Rotate thumb hand open (target thumb)
                       { 0, 60, -45, 0, 0, 0, 0, 0}, //Rotate thumb hand open (target thumb)
                       { 0, 15, -15, 0, 0, 0, 0, 0}, //Rotate thumb hand open (target thumb)
                       { 0, 0, 0, 0, 0, 0, 0, 0}, //Rotate thumb hand open (target thumb)
                       { -15, 90, -45, 0, -90, -90, -90, -90}, //Thumb over fist

                };
                //Define which sensors to use during training      
                ActiveSensor = new int[][] {
                    All,All,All,All,All,All,All,All
                    };
                
                
                   
//                Thumb  = new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 1, 1 };    //Thumb ch1
//                Index  = new int[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };    //Index ch2
//                Middle = new int[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
//                Ring   = new int[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 };
//                Pinky  = new int[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 };
//                
//                
//                //Define which sensors to use during training      
//                ActiveSensor = new int[][] {
//                    Thumb,Thumb,Thumb,Thumb,Index,Middle,Ring,Pinky
//                };
                break;

            case 4: //Gloves 5 Sensors
                   
//                Thumb  = new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //Thumb ch1
//                Index  = new int[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };    //Index ch2
//                Middle = new int[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0 };
//                Ring   = new int[] { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
//                Pinky  = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0 };

                TrainingY = new double[,] {
                   { 0,    0,   -20 , -20,   0,    0,   0,   0},         //Open fist
                   { 0, -20,    -20,  -40,   0,    0,   0,   0},         //Tuck thumb
                   { 0,  0, -40,   30, 90,  90, 90, 90},         //Close fist
                };
                //Define which sensors to use during training      
                //Define which sensors to use during training      
                ActiveSensor = new int[][] {
                    All,All,All,All,All,All,All,All
                };
                break;

        }

        TrainingYMax = getTrainingMax(TrainingY);
        TrainingYMin = getTrainingMin(TrainingY);
        currentY = getCurrentY(TrainingY);

        TrainingPositions = TrainingY.GetLength(0);
        nOutputs = TrainingY.GetLength(1);

        nSamples = samplesPerPosition * TrainingPositions;

        //Results - to pass to regression fit eq
        X = new double[nSamples, nSensors + 1];
        y = new double[nSamples, nOutputs];

    }

    private double[] getTrainingMin(double[,] TrainingY) {

        double[] TrainY = new double[TrainingY.GetLength(1)];

        for (int i = 0; i < TrainingY.GetLength(1); i++) {
            TrainY[i] = TrainingY[0,i];
            for (int j = 0; j < TrainingY.GetLength(0); j++)
            {
                if (TrainY[i] > TrainingY[j, i])
                {
                    TrainY[i] = TrainingY[j, i];
                }
            }
        }

        return TrainY;
    }

    private double[] getTrainingMax(double[,] TrainingY)
    {

        double[] TrainY = new double[TrainingY.GetLength(1)];

        for (int i = 0; i < TrainingY.GetLength(1); i++)
        {
            TrainY[i] = TrainingY[0, i];
            for (int j = 0; j < TrainingY.GetLength(0); j++)
            {
                if (TrainY[i] < TrainingY[j, i])
                {
                    TrainY[i] = TrainingY[j, i];
                }
            }
        }

        return TrainY;
    }

    private double[] getCurrentY(double[,] TrainingY) {

        double[] current = new double[TrainingY.GetLength(1)];

        for (int i = 0; i < TrainingY.GetLength(1); i++) {
            current[i] = TrainingY[0,i];
        }

        return current;

    }

    public void RecordNow()
    {
        //Require 2 presses to start first capture (TODO determine more stable way to manage first press)
        if (index == 0 && FirstPress)
        {
            FirstPress = false;
        }
        else
        {
            CaptureCalibrationData = true;
        }
    }

    public double[,] getTrainingData()
    {
        return TrainingY;
    }

    // Update is called once per frame
    public void Update(int[] currentX)
    {

        if (complete)
        {
            return;
        }

        if (currentX == LastX)
        {
            return;
        }
        else
        {
            LastX = currentX;

            //Check if the user has stopped moving
            if (CaptureCalibrationData)
            {
          
                //Start recording data
                //Include DC offset
                X[index, 0] = 1;
                //Sensor data
                for (int i = 0; i < nSensors; i++)
                {
                    X[index, i + 1] = currentX[i];
                }

                for (int i = 0; i < nOutputs; i++)
                {
                    y[index, i] = currentY[i];
                }
                samplesIndex++; //Increment position samples index
                index++;    //Increment global samples index

                if (samplesIndex >= samplesPerPosition)
                {
                    //Update to new position
                    UpdatePosition();
                    //Reset Flags
                    CaptureCalibrationData = false;
                    //Reset counters
                    samplesIndex = 0;
                }
            }
        }

    }


    private void UpdatePosition()
    {
        //Has training aqusition completed
        if (complete)
        {
            return;
        }

        //Increment Training position
        TrainingIndex++;

        if (TrainingIndex < TrainingPositions)
        {
            for (int i = 0; i < nOutputs; i++)
            {
                currentY[i] = TrainingY[TrainingIndex, i];
            }
        }
        else
        {
            SolveLeastSquares Solver = new SolveLeastSquares();

            complete = true;
            //Use data to calculate fitting equation

            String storageKey = "Calibration";
            Solver.setActiveSensors(ActiveSensor);
            Vector<double>[] trainingData = Solver.Solve(X, y);
            PlayerPrefs.SetInt(storageKey, trainingData.Length);

            for (int index = 0; index < trainingData.Length; index++)
            {
                PlayerPrefsX.SetFloatArray(storageKey + index, Array.ConvertAll(trainingData[index].ToArray(), x => (float)x));
            }
            setTheta(trainingData);
        }


    }

    public double[] ApplyTransformation(int[] input, Vector<double>[] theta)
    {

        SolveLeastSquares Solver = new SolveLeastSquares();
        return Solver.ApplyTransformation(input, theta);
    }

    private Vector<double>[] mtheta;
    public void setTheta(Vector<double>[] theta)
    {

        mtheta = theta;

    }

    public Vector<double>[] getTheta()
    {

        return mtheta;
    }


    public double[] getCurrentPosition()
    {
        //Return current target so UI can be updated
        return currentY;
    }

    public bool isComplete()
    {
        //Has the data aqusition finished
        return complete;
    }

    public void setComplete(bool isComplete)
    {
        complete = isComplete;
    }
    public double[,] getX()
    {
        return X;
    }
    public double[,] getY()
    {
        return y;
    }

}