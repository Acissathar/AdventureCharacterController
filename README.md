# Adventure Character Controller

A character controller built off of Jan Ott's CharacterMovementFundamentals and inspired by old adventure games like Ocarina of Time, Majora's Mask, and Twilight Princess. The idea is to provide a rigidbody based controller for physics, but take a more streamlined approach to various movement elements like ladders and jumping, so that focus can be placed on a more tightly designed experience

## Getting Started

### Disclaimers

While this is inspired by games like Ocarina of Time, it is _not_ a full replacement for such a system. This strictly handles basic character movement, and is independent of any battle or camera system.

### Dependencies

* [Unity Helpers]([https://assetstore.unity.com/packages/tools/physics/scivolo-character-controller-170660](https://github.com/Acissathar/UnityHelpers))
  * A personal collection of re-usable functions and scripts.
* [InternalDebug](https://github.com/Acissathar/InternalDebug)
  * Strips out Debug messages from non-dev builds.
* Cinemachine 3.0
  * Cinemachine is only necessary for the Samples as they use it for Camera Control. If you do not import the samples, you are free to use your own camera system.

It does not matter where these are installed (package or source) but they need to be in the project somewhere.

* Note: This is mainly developed and tested with Unity 6, however, there is a conditional compile for setting the Rigidbody velocity so that it will work in earlier versions, but changes outside of that will not be made with lower versions in mind.

### Installing

#### Install via git URL

In Package Manager, add https://github.com/Acissathar/AdventureCharacterController.git as a custom gitpackage.

![image](https://github.com/user-attachments/assets/eb88d6e1-4910-487c-93e6-82f4e274dc1a)

<img width="1133" height="176" alt="image" src="https://github.com/user-attachments/assets/40d62cb6-350b-425a-94e0-a138fd91dae2" />

A sample scene is provided in the package as well, but must be manually imported from the Package Manager dialogue. Additionally, there are a few example prefabs included to help quick start.

#### Source

Download repo, and copy the Runtime and Editor folders into your Unity project to modify directly.

## Contact

[@Acissathar](https://twitter.com/Acissathar)

Join us in the (Unofficial) [ORKFramework discord!](https://discord.gg/Bafvu9wtvs) 

Project Link: [https://github.com/Acissathar/NavMesh-Cleaner/](https://github.com/Acissathar/NavMesh-Cleaner/)

## Version History

* 0.4
    * Add free climb support

* 0.3
    * Initial Public Release

## License

This project is licensed under the MIT License - see the LICENSE.md file for details

## Acknowledgements

[Jan Ott](https://github.com/Jan-Ott/CharacterMovementFundamentals)
- For open sourcing CharacterMovementFundamentals which I've taken and used as a base for this controller.
