using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class InputManager : MonoBehaviourSingletonPersistent<InputManager>
{
    private static PlayerControls playerControls;

    #region events
    public delegate void StartTouch(Vector2 position);
    public event StartTouch OnStartTouch;
    public delegate void EndTouch(Vector2 position);
    public event EndTouch OnEndTouch;
    #endregion

    private Camera mainCamera;
    public override void Awake()
    {
        playerControls = new PlayerControls();
        mainCamera = Camera.main;
        DontDestroyOnLoad(mainCamera);
        base.Awake();

    }
    private void OnEnable()
    {
        //playerControls.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        playerControls.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        playerControls.Enable();
    }

    private void Start()
    {
        playerControls.Touch.PrimaryTouch.started += ctx => StartTouchPrimary();
        playerControls.Touch.PrimaryTouch.canceled += ctx => EndTouchPrimary();
    }

    void StartTouchPrimary() 
    {
        Debug.Log("StartTouchPrimary");
        OnStartTouch?.Invoke(Utils.ScreentoWorld(mainCamera,playerControls.Touch.PrimaryPosition.ReadValue<Vector2>()));
    }

    void EndTouchPrimary()
    {
        Debug.Log("EndTouchPrimary");
        OnEndTouch?.Invoke(Utils.ScreentoWorld(mainCamera, playerControls.Touch.PrimaryPosition.ReadValue<Vector2>()));
    }

    public Vector2 PrimaryPosition()
    {
        //return Utils.ScreentoWorld(mainCamera, playerControls.Touch.PrimaryPosition.ReadValue<Vector2>());
        return playerControls.Touch.PrimaryPosition.ReadValue<Vector2>();
    }
}
