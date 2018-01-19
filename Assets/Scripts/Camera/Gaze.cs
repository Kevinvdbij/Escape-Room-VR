﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Gaze : MonoBehaviour {
	// Static variables
    public static Gaze controller;
    public static Camera mainCamera;

    // Private variables visible in the inspector
	[SerializeField] private LayerMask gazeMask = 8;
    [SerializeField] private float gazeRange = 5F;
    [SerializeField] private float updateRate = 0.1F;

    [SerializeField] private GameObject reticleCanvas;
    [SerializeField] private float reticleDefaultDistance;
    [SerializeField] private Image reticleFill;

    [SerializeField] private Animator reticleAnimator;
    [SerializeField] private bool isReticleLerping;
    [SerializeField] private float reticleLerp;
    [SerializeField] float prevRetFill;
    [SerializeField] float retLerpTime;

    [SerializeField] private bool mouseControlEnabled;

    // Private variables hidden in the inspector
    private RaycastHit gazeHit;
    private InteractableObject hitObject;
    private InteractableObject lastObject;
    private Vector3 reticleScale;
    private Quaternion reticleRotation;

    // Is called when the script instance is being loaded
    public void Awake()
    {
        controller = this;
        mainCamera = Camera.main;

        reticleScale = reticleCanvas.transform.localScale;
        reticleRotation = reticleCanvas.transform.rotation;
        reticleDefaultDistance = mainCamera.farClipPlane;
    }

    // Is called every frame, if the MonoBehaviour is enabled
    private void Update()
    {
        GazeUpdate();
        GazeRaycast(Time.deltaTime);
        MouseControl();
    }

	// Casts a raycast from the camera and sets hitObject
    public void GazeRaycast(float elapsedTime)
    {
        if (Physics.Raycast(transform.position, transform.forward, out gazeHit, gazeRange, gazeMask))
        {
            hitObject = gazeHit.transform.GetComponent<InteractableObject>();
            UpdateReticle(true);

            if (hitObject)
                lastObject = hitObject;
        }
        else
        {
            hitObject = null;
            UpdateReticle(false);
        }
    }

	// Call IsActivated() on the IObject that is being gazed at after the duration
    public void GazeUpdate()
    {
		if (lastObject && IsGazingAt(lastObject))
        {
        	hitObject.HitDuration += Time.deltaTime;
            if (hitObject.HitDuration >= hitObject.activationDuration)
            {
                hitObject.IsActivated();
            }
         }
    }

    // Sets the reticle position to the raycast hit point and adjusts the scale
    public void UpdateReticle(bool isRaycasting)
    {
        if (isRaycasting)
        {
            reticleCanvas.transform.position = gazeHit.point;
            reticleCanvas.transform.localScale = reticleScale * gazeHit.distance;

            if (IsGazing)
            {
                if (reticleAnimator)
                    reticleAnimator.SetBool("isGazing", true);

                if (lastObject && lastObject != hitObject)
                {
                    isReticleLerping = true;
                    reticleLerp = 0;
                    prevRetFill = lastObject.hitDuration / lastObject.activationDuration;
                }

                float currentFill = hitObject.hitDuration / hitObject.activationDuration;
                if (!isReticleLerping)
                {
                    reticleFill.fillAmount = currentFill;
                }
                else
                {
                    reticleFill.fillAmount = Mathf.Lerp(prevRetFill, currentFill, reticleLerp);
                    reticleLerp += Time.deltaTime / retLerpTime;
                }
            }
            else
            {
                if (reticleAnimator)
                    reticleAnimator.SetBool("isGazing", false);

                if (lastObject)
                {
                    reticleFill.fillAmount = lastObject.hitDuration / lastObject.activationDuration;
                }
            }

        }
        else
        {
            reticleCanvas.transform.position = mainCamera.transform.position + mainCamera.transform.forward * reticleDefaultDistance;
            reticleCanvas.transform.localScale = reticleScale * reticleDefaultDistance;
        }
    }

    // Returns true if the camera is looking at an IObject
    public bool IsGazing
    {
        get
        {
            if (hitObject)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    // Returns true when the IObject parameter is being looked at
    public bool IsGazingAt(InteractableObject iObject)
    {
        if (hitObject && hitObject == iObject)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Mouse controls for use in editor
    public void MouseControl()
    {
        if (!mouseControlEnabled) return;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float xRotation = mainCamera.transform.rotation.eulerAngles.x + -Input.GetAxis("Mouse Y");
            float yRotation = mainCamera.transform.rotation.eulerAngles.y + Input.GetAxis("Mouse X");
            mainCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
    }
}