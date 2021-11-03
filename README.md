# A Walk in the Black-Box: Deep Convolutional Neural Network Visualization in Virtual Reality - Client

This repository visualizes deep convolutional neural networks (CNNs) in 3D. Take a walk in your CNN and interact with it to get a more intuitive understanding of Deep Learning or to analyze your models. The software provides visualization algorithms like feature visualization to reveal what visual concepts the models have learned. Virtual Reality and desktop mode are available. Currently, CNNs for image classification and segmentation are supported. 

The repository consists of the client and the server part. The client contains the Unity project to display and to interact with the networks. The server is a Python based implementation, handles the networks and provides the client with data. The server can be run in Docker and an appropriate Dockerfile is included in the repository. 

This software is a part of the [DeepVisionVR](https://github.com/Criscraft/DeepVisionVR) project.


## Requirements

- Unity 2021.1.14f1
Get the [Server](https://github.com/Criscraft/DeepVisionVRServer) to use the visualization software.


## Get started

Install the most recent release of the client and run it. There is a version for VR and for desktop mode, respectively. Before running the client, make sure that the server is running.

If you would like to modify the client download [Unity Hub](https://unity3d.com/get-unity/download) and install Unity 2021.1.14f1
If you prefer to use desktop mode, load the scene SubSceneNonVR. For VR load the scene SubSceneVR.