﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VisualGuideGameMode
{
    Lobby,
    Play,
    Reward
}

public class VisualGuideManager : MonoBehaviour 
{
    public VisualGuideGameMode currentMode = VisualGuideGameMode.Lobby;
    public MitosisGameManager currentGameManager;

    string[] structureNames = { "Endoplasmic Reticulum", "Golgi Apparatus", "Microtubules", "Mitochondria"};
    Dictionary<string,bool> structuresSolved;
    Animator mitoticCellsAnimation;

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
    public InterphaseCellManager interphaseCell
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

    bool allStructuresSolved
    {
        get
        {
            foreach (KeyValuePair<string,bool> kvp in structuresSolved)
            {
                if (kvp.Value == false)
                {
                    return false;
                }
            }
            return true;
        }
    }

    void Start ()
    {
        ResetSolvedStructures();
    }

    public void ResetSolvedStructures ()
    {
        structuresSolved = new Dictionary<string, bool>();
        foreach (string structure in structureNames)
        {
            structuresSolved.Add( structure, false );
        }
        interphaseCell.GrayOutStructures();
    }

    public void StartGame (string structureName)
    {
        currentMode = VisualGuideGameMode.Play;

        Cleanup();
        CreateMitosisGameManager();
        currentGameManager.StartGame( structureName, 1.5f );
        interphaseCell.TransitionToPlayMode( currentGameManager );
        structuresSolved[structureName] = false;
        //ControllerInput.Instance.ToggleLaserRenderer( false );
        UIManager.Instance.ToggleResetButton( false );
        UIManager.Instance.ToggleBackButton( true );
    }

    void CreateMitosisGameManager ()
    {
        GameObject prefab = Resources.Load( "MitosisGame" ) as GameObject;
        if (prefab == null)
        {
            Debug.LogWarning( "Couldn't load prefab for MitosisGame" );
            return;
        }
        currentGameManager = (Instantiate( prefab ) as GameObject).GetComponent<MitosisGameManager>();
    }

    public void StartSuccessAnimation ()
    {
        interphaseCell.MoveToCenter( 1f );
    }

    public void TriggerMitoticCellsAnimation ()
    {
        GameObject prefab = Resources.Load( currentGameManager.currentStructureName + "/MitoticCells" ) as GameObject;
        if (prefab == null)
        {
            Debug.LogWarning( "Couldn't load prefab for " + currentGameManager.currentStructureName + " MitoticCells!" );
        }
        mitoticCellsAnimation = (Instantiate( prefab, transform.position, transform.rotation, transform ) as GameObject).GetComponent<Animator>();
        mitoticCellsAnimation.SetTrigger( "Play" );
    }

    public void FinishSuccessAnimation ()
    {
        string structureName = currentGameManager.currentStructureName;
        structuresSolved[structureName] = true;

        ReturnToLobby( structureName );
    }

    public void ReturnToLobby (string structureJustSolved = null)
    {
        currentMode = VisualGuideGameMode.Lobby;

        Cleanup();

        interphaseCell.gameObject.SetActive( true );
        interphaseCell.TransitionToLobbyMode( structureJustSolved );
        ControllerInput.Instance.ToggleLaserRenderer( true );
        UIManager.Instance.ToggleBackButton( false );
        UIManager.Instance.ToggleResetButton( true );
    }

    public void CheckSetupReward ()
    {
        if (allStructuresSolved)
        {
            currentMode = VisualGuideGameMode.Reward;

            interphaseCell.gameObject.SetActive( false );
            CreateMitosisGameManager();
            StartCoroutine( currentGameManager.SpawnAllThrowables( structureNames ) );
            StartCoroutine( EndReward() );
        }
    }

    IEnumerator EndReward ()
    {
        yield return new WaitForSeconds( 10f );

        if (currentMode == VisualGuideGameMode.Reward)
        {
            currentMode = VisualGuideGameMode.Lobby;

            Cleanup();
            interphaseCell.gameObject.SetActive( true );
        }
    }

    void Cleanup ()
    {
        if (currentGameManager != null)
        {
            Destroy( currentGameManager.gameObject );
        }
        if (mitoticCellsAnimation != null)
        {
            Destroy( mitoticCellsAnimation.gameObject );
        }
    }
}
