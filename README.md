# Glove UnityDemo


This UnityDemo allows to connect with StretchSense glove and see how sensor data is translated into animation. To make it work following is required:
  - A StretchSense Glove
  - Ble connection script from Unity asset store : https://www.google.com/url?q=https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661&sa=D&source=hangouts&ust=1545102703884000&usg=AFQjCNH6VE8wO31RKimOKx-RekzcWnB4Ng
  - Any 3D asset like glove or hand. We have included free asset if you import GloveDemo scene. If you import your new asset then you may have to define your reference position as default one may not work. (Please see TrainingData.cs TrainingY array)

# How to make it work!

  - UnityDemo/Assets/Scripts/General SSL Bluetooth
 
Script within these folders are responsible for making Ble connection and receiving notifications once connected. They will not work without the paid ble script from unity assest store.
  
  - UnityDemo/Assets/Scripts/GloveUIManager.cs

This Script is responsible for taking UI input and respond to actions.

  - UnityDemo/Assets/Scripts/Controller/HandController.cs
  
This script listens to notification data from ble and pass it to machine learning scripts which returns meaningful data that can be used to handle glove animation  

   - UnityDemo/Assets/Scripts/Learning and Calibration
   
The folder within the folder are responsible to collect data at different calibrating positions and run machine learning algorithm on them

# Demo Video

https://www.youtube.com/watch?v=vc86d-5sAlY
