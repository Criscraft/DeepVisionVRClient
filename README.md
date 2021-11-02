![Title](Panorama.png)

# A Walk in the Black-Box: Deep Convolutional Neural Network Visualization in Virtual Reality - Client

This repository visualizes deep convolutional neural networks (CNNs) in 3D. Take a walk in your CNN and interact with it to get a more intuitive understanding of Deep Learning or to analyze your models. The software provides visualization algorithms like feature visualization to reveal what visual concepts the models have learned. Virtual Reality and desktop mode are available. Currently, CNNs for image classification and segmentation are supported. 

The repository consists of the client and the server part. The client contains the Unity project to display and to interact with the networks. The server is a Python based implementation, handles the networks and provides the client with data. The server can be run in Docker and an appropriate Dockerfile is included in the repository. 


## Requirements

Unity 2021.1.14f1


## Get started

### Client:
Download [Unity Hub](https://unity3d.com/get-unity/download) and install Unity 2021.1.14f1  
If you prefer to use desktop mode, load the scenes BaseScene and SubSceneNonVR. Otherwise load the BaseScene together with SubSceneVR. Start the server separatedly and run the application inside Unity. 


## Results

![Network](Netzwerk.png)
![Architektur](Architektur.png)
![Feature Visualisierung](FeatureVisualisierung.png)