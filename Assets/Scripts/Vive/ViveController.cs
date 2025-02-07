﻿using UnityEngine;
using System.Collections;
using Valve.VR;

public enum DPadDirection
{
	Up,
	Left,
	Right,
	Down
}

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class ViveController : MonoBehaviour
{
	public bool useDPadPress;
	public bool useDPadHover;
	public bool useTriggerInput;

	SteamVR_TrackedObject trackedObj;
	SteamVR_Controller.Device controller
	{
		get
		{
			if (trackedObj == null)
			{
				trackedObj = GetComponent<SteamVR_TrackedObject>();
			}
			return SteamVR_Controller.Input( (int)trackedObj.index );
		}
	}

	Vector2 dPadPosition;
	bool[] dPadHovering = new bool[4];

	// Update is called once per frame
	void Update ()
	{
		GetInput();
		DoUpdate();
	}

	protected virtual void DoUpdate () { }

	void GetInput ()
	{
		if (controller == null)
		{
			Debug.LogWarning("Vive controller not initialized");
			return;
		}

		if (useDPadHover)
		{
			GetDPadHover();
		}
		if (useDPadPress)
		{
			GetDPadPress();
		}
		if (useTriggerInput)
		{
			GetTrigger();
		}
	}

	void GetDPadHover ()
	{
		dPadPosition = controller.GetAxis();

		if (controller.GetTouchDown( EVRButtonId.k_EButton_SteamVR_Touchpad ))
		{
			OnDPadEnter();
		}

        if (controller.GetTouchUp(EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            GetDPadExit();
        }

        if (dPadPosition.y > 0.4f)
		{
			if (!dPadHovering[(int)DPadDirection.Up])
			{
				GetDPadExit();
				dPadHovering[(int)DPadDirection.Up] = true;
				OnDPadUpEnter();
			}
		}
		else if (dPadPosition.y < -0.4f)
		{
			if (!dPadHovering[(int)DPadDirection.Down])
			{
				GetDPadExit();
				dPadHovering[(int)DPadDirection.Down] = true;
				OnDPadDownEnter();
			}
		}
		else if (dPadPosition.x < -0.4f)
		{
			if (!dPadHovering[(int)DPadDirection.Left])
			{
				GetDPadExit();
				dPadHovering[(int)DPadDirection.Left] = true;
				OnDPadLeftEnter();
			}
		}
		else if (dPadPosition.x > 0.4f)
		{
			if (!dPadHovering[(int)DPadDirection.Right])
			{
				GetDPadExit();
				dPadHovering[(int)DPadDirection.Right] = true;
				OnDPadRightEnter();
			}
		}
	}

	void GetDPadExit ()
	{
		for (int i = 0; i < dPadHovering.Length; i++)
		{
			if (dPadHovering[i])
			{
				dPadHovering[i] = false;
				OnDPadExit();
			}
		}
	}

	void GetDPadPress ()
	{
		if (controller.GetPressUp(EVRButtonId.k_EButton_SteamVR_Touchpad))
		{
            OnDPadPressed();
			if (dPadHovering[(int)DPadDirection.Up])
			{
				OnDPadUpPressed();
			}
			else if (dPadHovering[(int)DPadDirection.Left])
			{
				OnDPadLeftPressed();
			}
			else if (dPadHovering[(int)DPadDirection.Right])
			{
				OnDPadRightPressed();
			}
			else if (dPadHovering[(int)DPadDirection.Down])
			{
				OnDPadDownPressed();
			}
		}
        if (controller.GetPress(EVRButtonId.k_EButton_SteamVR_Touchpad))
        {
            if (dPadHovering[(int)DPadDirection.Up])
            {
                OnDPadUpStay();
            }
            else if (dPadHovering[(int)DPadDirection.Left])
            {
                OnDPadLeftStay();
            }
            else if (dPadHovering[(int)DPadDirection.Right])
            {
                OnDPadRightStay();
            }
            else if (dPadHovering[(int)DPadDirection.Down])
            {
                OnDPadDownStay();
            }
        }
    }

	void GetTrigger ()
	{
		if (controller.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
		{
			OnTriggerPull();
		}
        if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
        {
            OnTriggerHold();
        }
        if (controller.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            OnTriggerRelease();
        }
    }

    public virtual void OnDPadEnter() { }

    public virtual void OnDPadUpEnter () { }

	public virtual void OnDPadLeftEnter () { }

	public virtual void OnDPadRightEnter () { }

	public virtual void OnDPadDownEnter () { }

	public virtual void OnDPadExit () { }

    public virtual void OnDPadPressed() { }

    public virtual void OnDPadUpPressed () { }

	public virtual void OnDPadLeftPressed () { }

	public virtual void OnDPadRightPressed () { }

	public virtual void OnDPadDownPressed () { }

    public virtual void OnDPadUpStay() { }

    public virtual void OnDPadLeftStay() { }

    public virtual void OnDPadRightStay() { }

    public virtual void OnDPadDownStay() { }

    public virtual void OnTriggerPull () { }

	public virtual void OnTriggerHold () { }

    public virtual void OnTriggerRelease() { }
}