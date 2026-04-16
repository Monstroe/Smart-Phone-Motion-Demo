# Smart Phone Motion Demo

## Introduction

While motion-controlled experiences are well established, they typically require the purchase of dedicated external hardware (controllers, base stations, remotes, etc.). However, modern smartphones already come equipped with all the necessary hardware, such as IMUs, cameras, and Augmented Reality (AR) capabilities. In this project, a standard smartphone is utilized as a motion controller that connects to a separate desktop computer, allowing the user to play motion-controlled experiences. All that is required are two typical consumer electronic devices, a desktop/laptop and a smartphone.

To evaluate this concept, a 3D simulation was developed in which users can interact with virtual objects using motion input from a smartphone. The system supports object selection, manipulation, and movement with real time orientation tracking. While the concept is simple in nature, it is more than capable of determining the feasibility of this technology.

## Development Platform

The simulation is an executable program that runs on a typical Windows computer, and the smartphone is a standard Android cell phone. The mobile device itself has a custom application to provide a control interface to the user and communicate with the Windows PC. Both programs were developed using the Unity game engine. Unity was selected due to its cross-platform capabilities, extensive support for both mobile and desktop development, and built-in simulation tools for handling rendering, physics, and input.

Unity also provides direct access to the sensors on the mobile device, including the IMU, through its input system. No low-level hardware interfacing was required. Its real time renderer, networking capabilities, and physics engine make it an ideal tool for building a real time, cross-platform, interactive simulation, such as the one developed in this project.

## System Architecture

The overall architecture follows the client-server model. The mobile application is a lightweight client that collects sensor data, while the desktop application acts as the centralized server that handles the mobile input and displays the simulation.

- Mobile Client
  - Captures IMU-based orientation data
  - Detects user input (button hold/release and drag gestures)
  - Sends data to the server at 60 Hz
- Desktop Server
  - Receives motion and input data from the mobile client
  - Applies attitude transformations to a virtual controller object (this is what controls the virtual “laser pointer”)
  - Performs physics simulation and interaction handling

Ensuring most of the processing and simulation happens on the server means the mobile application can remain lightweight while the computationally intensive tasks such as physics simulation and collision detection remain on the desktop application.

## Data Communication
Communication between the mobile client and desktop server is implemented using a lightweight networking library ([CNet](https://github.com/Monstroe/CNet)) designed to seamlessly provide both TCP and UDP communication through one simplistic interface. This reduced implementation complexity and meant full focus could be put on application features instead of writing low-level socket communication logic. UDP transmission was specifically selected for this project due to its low latency.

An additional abstraction layer is used to manage higher level networking concepts such as packet construction, message handling via custom services, and networked object synchronization ([CNS](https://github.com/Monstroe/CNetworkingSolution)). This modular design allows different networking technologies (called “transports”) to be integrated without modifying any core application logic. For example, alternative communication methods such as Bluetooth Classic could be easily used so long as a compatible transport layer is implemented. The system architecture is very flexible for this reason and could be easily extended in the future.

## Conclusion
This project successfully demonstrates the feasibility of using a standard smartphone as a motion controller for a computer simulation. Instead of requiring external devices and hardware, using built-in mobile sensors and standard wireless communication protocols can provide real time motion interaction to desktop applications. The developed prototype supports object selection, grabbing, and motion control using IMU data, networking, and the Unity physics engine. Despite limitations with sensor accuracy and the lack of positional tracking, the system still provides a responsive and intuitive user experience.

Overall, the results indicate that smartphones can serve as viable motion controllers for interactive simulations. This approach has the potential to increase accessibility and reduce hardware costs across a variety of industries, such as gaming, training, and rehabilitation systems.
