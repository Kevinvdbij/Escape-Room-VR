﻿using UnityEngine;
using System.Collections;
public class GyroRotate : MonoBehaviour
{
    private Gyroscope gyro;
    [SerializeField] private GameObject player;

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
        }
        else
        {
            Debug.Log("Phone doesen't support");
        }
    }

    void Update()
    {
        transform.Rotate(-Input.gyro.rotationRateUnbiased.x, -Input.gyro.rotationRateUnbiased.y, 0);
    }
}
