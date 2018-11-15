﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VisualGuideGameMode
{
    Lobby,
    Play
}

public class VisualGuideManager : MonoBehaviour 
{
    public VisualGuideGameMode currentMode = VisualGuideGameMode.Lobby;

    MitosisGameManager currentGameManager;
    Dictionary<string,bool> structuresSolved;

    static VisualGuideManager _Instance;
    public static VisualGuideManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<VisualGuideManager>();
            }
            return _Instance;
        }
    }

    InterphaseCellManager _interphaseCell;
    InterphaseCellManager interphaseCell
    {
        get
        {
            if (_interphaseCell == null)
            {
                _interphaseCell = GameObject.FindObjectOfType<InterphaseCellManager>();
            }
            return _interphaseCell;
        }
    }

    void Start ()
    {
        SetupPuzzles();
    }

    void SetupPuzzles ()
    {
        structuresSolved = new Dictionary<string, bool>();
        CellStructure[] structures = GetComponentsInChildren<CellStructure>();
        foreach (CellStructure structure in structures)
        {
            structuresSolved.Add( structure.structureName, false );
        }
    }

    public void StartGame (string structureName)
    {
        if (currentMode == VisualGuideGameMode.Lobby)
        {
            currentMode = VisualGuideGameMode.Play;
            CreateMitosisGameManager( structureName );
            interphaseCell.TransitionToPlayMode( currentGameManager );
            structuresSolved[structureName] = false;
            ControllerInput.Instance.ToggleLaserRenderer( false );
        }
    }

    void CreateMitosisGameManager (string structureName)
    {
        GameObject prefab = Resources.Load( "MitosisGame" ) as GameObject;
        if (prefab == null)
        {
            Debug.LogWarning( "Couldn't load prefab for MitosisGame" );
            return;
        }
        currentGameManager = (Instantiate( prefab ) as GameObject).GetComponent<MitosisGameManager>();
        currentGameManager.StartGame( structureName, 3f );
    }

    public void CompleteGame (string structureName)
    {
        if (currentMode == VisualGuideGameMode.Play)
        {
            currentMode = VisualGuideGameMode.Lobby;
            Destroy( currentGameManager.gameObject );
            interphaseCell.TransitionToLobbyMode( structureName );
            structuresSolved[structureName] = true;
            ControllerInput.Instance.ToggleLaserRenderer( true );
        }
    }
}
