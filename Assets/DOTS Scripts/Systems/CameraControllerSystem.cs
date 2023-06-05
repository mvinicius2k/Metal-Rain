using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

//public partial struct CameraControllerSystem : ISystem
//{
//    private Vector3 lastMousePosition;
//    public void OnUpdate(ref SystemState state)
//    {
//        if (CameraSingleton.Instance == null)
//            return;

//        var deltaTime = SystemAPI.Time.DeltaTime;

//        var inputVector = new Vector3
//        {
//            x = Input.GetAxis(Constants.InputHorizontal),
//            z = Input.GetAxis(Constants.InputFoward)
//        };

//        //Movimentação da camera

//        var moveSpeed = CameraSingleton.Instance.MoveSensibility;
//        var camOffset = new Vector3
//        {
//            x = inputVector.x * deltaTime * moveSpeed,
//            z = inputVector.z * deltaTime * moveSpeed,
//        };

//        if (camOffset != Vector3.zero)
//            CameraSingleton.Instance.transform.Translate(camOffset, Space.Self);

//        //Rotação da camera

//        if (Input.GetButtonDown(Constants.InputRotateCam))
//        {
//            lastMousePosition = Input.mousePosition; //Obtendo ponto de partida para o deslocamento do mouse
//        }
//        if (Input.GetButton(Constants.InputRotateCam))
//        {
//            var mouseOffset = lastMousePosition - Input.mousePosition;
//            if (mouseOffset != Vector3.zero)
//            {
//                var rotateSpeed = CameraSingleton.Instance.RotationSensibility;
//                var currentRotation = CameraSingleton.Instance.transform.localEulerAngles;
//                var rotateVector = new Vector3
//                {
//                    x = currentRotation.x + mouseOffset.y * rotateSpeed * deltaTime,
//                    y = currentRotation.y - mouseOffset.x * rotateSpeed * deltaTime,
//                    z = 0f,
//                };
//                CameraSingleton.Instance.transform.localEulerAngles = rotateVector;
//            }

//            lastMousePosition = Input.mousePosition;
//        }


//    }
//}
