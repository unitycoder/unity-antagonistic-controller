﻿/****************************************************
 * File: RigidBodyController.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Animation and beyond
   * Last update: 04/07/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RigidBodyController : MonoBehaviour
{
    #region Read-only & Static Fields

    private Rigidbody _body;
    private Animator _anim;

    #endregion

    #region Instance Fields

    [Header("Input - Options")]
    public bool joystickMode = true;
    public bool fightMode = true;

    [Header("Motion - Options")]
    public Transform rootKinematicSkeleton;
    public float moveSpeed = 1.0f;
    public float offsetKinematicMovement = 1f;
    public bool shooterCameraMode = false;
    public float rotationSpeed = 280f;
    public bool blockCamera = false;
    public bool moveForwardOnly = false;

    [Header("Motion - Debug")]
    public Vector3 _inputs = Vector3.zero;
    public Vector3 moveDirection;
    public float inputMagnitude;
    public Vector3 _velocity;

    [Header("Ground - Options")]
    public Transform _groundChecker;
    public float GroundDistance = 0.2f;
    public LayerMask Ground;

    [Header("Ground - Debug")]
    public bool _isGrounded = true;

    #endregion

    #region Unity Methods

    void Start()
    {
        // Retrieve components
        _body = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // Check if grounded
        _isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);

        if (_body.isKinematic)
        {
            transform.position += moveDirection * moveSpeed * offsetKinematicMovement * inputMagnitude * Time.deltaTime;

            _velocity.y += Physics.gravity.y * Time.fixedDeltaTime;

            if (_isGrounded && _velocity.y < 0)
                _velocity.y = 0f;
        }
    }

    private void Update()
    {
        // User-input
        _inputs = Vector3.zero;
        _inputs.x = Input.GetAxis("Horizontal");
        _inputs.z = Input.GetAxis("Vertical");

        // Direction of the character with respect to the input (e.g. W = (0,0,1))
        moveDirection = Vector3.forward * _inputs.z + Vector3.right * _inputs.x;

        // Rotate with respect to the camera: Calculate camera projection on ground -> Change direction to be with respect to camera
        Vector3 projectedCameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        Quaternion rotationToCamera = Quaternion.LookRotation(projectedCameraForward, Vector3.up);
        moveDirection = rotationToCamera * moveDirection;

        // How to rotate the character: In shooter mode, the character rotates such that always points to the forward of the camera
        if (shooterCameraMode)
        {
            if (_inputs != Vector3.zero)
            {
                if (!blockCamera)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToCamera, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (_inputs != Vector3.zero && !moveForwardOnly)
            {
                Quaternion rotationToMoveDirection = Quaternion.LookRotation(moveDirection, Vector3.up);
                if (!blockCamera)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToMoveDirection, rotationSpeed * Time.deltaTime);
            }
        }

        _anim.SetBool("JoystickMode", joystickMode);
        _anim.SetFloat("InputX", _inputs.x, 0.0f, Time.deltaTime);
        _anim.SetFloat("InputZ", _inputs.z, 0.0f, Time.deltaTime);

        _inputs.Normalize();
        inputMagnitude = _inputs.sqrMagnitude;

        if (!joystickMode)
        {
            if (fightMode)
            {
                if (Input.GetKey(KeyCode.Space))
                    _anim.SetBool("isRunning", true);
                else
                    _anim.SetBool("isRunning", false);
            }

            if (_inputs != Vector3.zero)
            {
                _anim.SetBool("isWalking", true);

                if (Input.GetKey(KeyCode.Space))
                    _anim.SetBool("isRunning", true);
                else
                    _anim.SetBool("isRunning", false);
            }
            else
            {
                _anim.SetBool("isWalking", false);
            }

            _anim.SetFloat("SpeedAnimation", moveSpeed, 0.0f, Time.deltaTime);
        }
        else
        {
            _anim.SetFloat("InputMagnitude", inputMagnitude, 0.0f, Time.deltaTime);
            _anim.SetFloat("SpeedAnimation", moveSpeed, 0.0f, Time.deltaTime);
        }
    }
    
    #endregion
}
