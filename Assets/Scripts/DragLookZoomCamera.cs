﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Cameras;
using UnityEngine.EventSystems;

public class DragLookZoomCamera : FreeLookCam 
{
	public float zoomSpeedScroll = 1f;
	public float zoomSpeedArrows = 0.1f;
	public Vector2 zoomLimits = new Vector2( -5f, -50f );

	new void Update () 
	{
		DragToLook();
		ScrollToZoom();
	}

	void DragToLook ()
	{
		if (CrossPlatformInputManager.GetButton( "Fire1" ) && !EventSystem.current.IsPointerOverGameObject())
		{
			HandleRotationMovement();
		}
	}

	void ScrollToZoom ()
	{
		float scroll = CrossPlatformInputManager.GetAxis( "Mouse ScrollWheel" );
		float arrow = CrossPlatformInputManager.GetAxis( "Vertical" );

		float speed = zoomSpeedScroll;
		if (arrow != 0)
		{
			speed = zoomSpeedArrows;
		}

		if (scroll > 0 || arrow > 0)
		{
			if (m_Cam.localPosition.z < zoomLimits.x)
			{
				m_Cam.localPosition += speed * Vector3.forward;
			}
		}
		else if (scroll < 0 || arrow < 0)
		{
			if (m_Cam.localPosition.z > zoomLimits.y)
			{
				m_Cam.localPosition -= speed * Vector3.forward;
			}
		}
	}
}