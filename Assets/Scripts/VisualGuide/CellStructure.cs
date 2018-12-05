﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CellStructure : VRTK_InteractableObject 
{
    [Header("Cell Structure Settings")]

    public bool isNucleus;
    public string structureName;
    public float nameWidth = 80f;

    InterphaseCellManager _interphaseCell;
    InterphaseCellManager interphaseCell
    {
        get
        {
            if (_interphaseCell == null)
            {
                _interphaseCell = GetComponentInParent<InterphaseCellManager>();
            }
            return _interphaseCell;
        }
    }

    Collider _collider;
    public Collider theCollider
    {
        get
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
            }
            return _collider;
        }
    }

    Colorer _colorer;
    public Colorer colorer
    {
        get
        {
            if (_colorer == null)
            {
                _colorer = GetComponent<Colorer>();
            }
            return _colorer;
        }
    }

    public VRTK_DestinationMarker laserPointer
    {
        get
        {
            return ControllerInput.Instance == null ? null : ControllerInput.Instance.laserPointer;
        }
    }

    protected override void Awake ()
    {
        base.Awake();
        colorer.SetColor( 0 );
    }

    protected override void OnEnable ()
    {
        base.OnEnable();
        if (laserPointer != null)
        {
            laserPointer.DestinationMarkerEnter += OnHoverEnter;
            laserPointer.DestinationMarkerExit += OnHoverExit;
        }
    }

    protected override void OnDisable ()
    {
        base.OnDisable();
        if (laserPointer != null)
        {
            laserPointer.DestinationMarkerEnter -= OnHoverEnter;
            laserPointer.DestinationMarkerExit -= OnHoverExit;
        }
    }

    void OnHoverEnter (object sender, DestinationMarkerEventArgs e)
    {
        if (theCollider == e.raycastHit.collider)
        {
            interphaseCell.LabelStructure( this );
        }
    }

    void OnHoverExit (object sender, DestinationMarkerEventArgs e)
    {
        if (theCollider == e.raycastHit.collider)
        {
            interphaseCell.HideLabel( this );
        }
    }

    public override void StartUsing (VRTK_InteractUse currentUsingObject = null)
    {
        base.StartUsing( currentUsingObject );

        if (VisualGuideManager.Instance.currentMode == VisualGuideGameMode.Lobby)
        {
            Debug.Log( "select from geometry " + structureName );
            interphaseCell.SelectStructure( this );
        }
    }
}
