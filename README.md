# A Walk in the Black-Box: Deep Convolutional Neural Network Visualization in Virtual Reality - Client

The [DeepVisionVR](https://github.com/Criscraft/DeepVisionVR) project visualizes deep convolutional neural networks (CNNs) in 3D. Take a walk in your CNN and interact with it to get a more intuitive understanding of Deep Learning or to analyze your models.

This repository contains the *client* part of the project, which contains the Unity project to display and interact with artificial neural networks. Virtual Reality and desktop modes are available. The software uses the OpenXR framework. The Server can be found [HERE](https://github.com/Criscraft/DeepVisionVRServer).

## Requirements

Please install the most recent release of the client (see releases). There is a version for VR and for desktop mode, respectively.

For development:
Please install Unity 2021.1.14f1 using [Unity Hub](https://unity3d.com/get-unity/download). By default, the Unity project will start in non-VR mode. If you want to develop with VR enabled, please unload the scene SubSceneNonVR and load SubSceneVR. Then, go to Project Settings -> XR Plug-in Management and enable 'Initialize XR on Startup'.

## Get started

Simply run the client (either the VR-release or the desktop release). Before running the client, make sure that the server is set-up and running.