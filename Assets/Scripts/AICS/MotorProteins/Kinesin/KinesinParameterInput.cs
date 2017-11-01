﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AICS.MT;

namespace AICS.MotorProteins.Kinesin
{
	public class KinesinParameterInput : ParameterInput<KinesinParameterInput>  
	{
		public bool countFrameRate;
		public Kinesin kinesin;
		public Microtubule microtubule;
		public FollowLookZoomCamera cameraController;

		public Parameter[] rates;

		public Parameter timeMultiplier; // = 300x, 0 --> 10000
		public RangeParameter necklinkerLength; // = (1, 5), 1 --> 9
		public Parameter snappingSpeed; // = 90°/s, 5 --> 100
		public Parameter meanStepSize; // = 0.8 nm, 0.1 --> 2

		public Parameter averageWalkingSpeed; // μm/s

		public override void InitSliders () 
		{
			foreach (Parameter rate in rates)
			{
				rate.InitSlider();
			}
			timeMultiplier.InitSlider();
			necklinkerLength.InitSlider();
			snappingSpeed.InitSlider();
			meanStepSize.InitSlider();
		}

		public void SetRateA (float _sliderValue)
		{
			rates[0].Set( _sliderValue );
			kinesin.kineticRates.rates[0].rate = rates[0].value;
		}

		public void SetRateB (float _sliderValue)
		{
			rates[1].Set( _sliderValue );
			kinesin.kineticRates.rates[1].rate = rates[1].value;
		}

		public void SetRateC (float _sliderValue)
		{
			rates[2].Set( _sliderValue );
			kinesin.kineticRates.rates[2].rate = rates[2].value;
		}

		public void SetRateD (float _sliderValue)
		{
			rates[3].Set( _sliderValue );
			kinesin.kineticRates.rates[3].rate = rates[3].value;
		}

		public void SetRateE (float _sliderValue)
		{
			rates[4].Set( _sliderValue );
			kinesin.kineticRates.rates[4].rate = rates[4].value;
		}

		public void SetRateF (float _sliderValue)
		{
			rates[5].Set( _sliderValue );
			kinesin.kineticRates.rates[5].rate = rates[5].value;
		}

		public void SetRateG (float _sliderValue)
		{
			rates[6].Set( _sliderValue );
			kinesin.kineticRates.rates[6].rate = rates[6].value;
		}

		public void SetRateH (float _sliderValue)
		{
			rates[7].Set( _sliderValue );
			kinesin.kineticRates.rates[7].rate = rates[7].value;
		}

		public void SetRateI (float _sliderValue)
		{
			rates[8].Set( _sliderValue );
			kinesin.kineticRates.rates[8].rate = rates[8].value;
		}

		public void SetRateJ (float _sliderValue)
		{
			rates[9].Set( _sliderValue );
			kinesin.kineticRates.rates[9].rate = rates[9].value;
		}

		public void SetTimeMultiplier (float _sliderValue)
		{
			timeMultiplier.Set( 1f - _sliderValue );
			if (timeMultiplier.value < 2f)
			{
				timeMultiplier.displayValue.text = "0 " + timeMultiplier.units;
			}
			MolecularEnvironment.Instance.SetTime( timeMultiplier.value );
		}

		public void SetNecklinkerLengthMin (float _sliderValue)
		{
			necklinkerLength.SetMin( _sliderValue );
			kinesin.SetMinDistanceFromParent( necklinkerLength.value );
		}

		public void SetNecklinkerLengthMax (float _sliderValue)
		{
			necklinkerLength.SetMax( _sliderValue );
			kinesin.SetMaxDistanceFromParent( necklinkerLength.rangeValue );
		}

		public void SetSnappingSpeed (float _sliderValue)
		{
			snappingSpeed.Set( _sliderValue );
			kinesin.hips.snapSpeed = 1000f * snappingSpeed.value;
		}

		public void SetMeanStepSize (float _sliderValue)
		{
			meanStepSize.Set( _sliderValue );
			kinesin.SetMeanStepSize( meanStepSize.value );
		}

		public void ToggleSteerNecklinker (bool _toggleValue)
		{
			kinesin.hips.doSnap = _toggleValue;
		}

		public void ToggleStartBound (bool _toggleValue)
		{
			
		}

		public Text pauseButtonText;

		public void TogglePause ()
		{
			if (MolecularEnvironment.Instance.pause)
			{
				MolecularEnvironment.Instance.pause = false;
				pauseButtonText.text = "Pause";
			}
			else 
			{
				MolecularEnvironment.Instance.pause = true;
				pauseButtonText.text = "Play";
			}
		}

		public void Reset ()
		{
			kinesin.Reset();
			microtubule.DoReset();
			MolecularEnvironment.Instance.Reset();
			cameraController.LookAtTarget();
		}

		public void ResetRates ()
		{
			foreach (Parameter rate in rates)
			{
				rate.ResetSlider();
			}
		}

		public Text fpsDisplay;
		public Text spfDisplay;
		float lastTime = -1f;

		void Update ()
		{
			if (countFrameRate && Time.time - lastTime > 0.3f)
			{
				fpsDisplay.text = Mathf.Round(1f / Time.deltaTime).ToString() + " frames per second";
				spfDisplay.text = MolecularEnvironment.Instance.stepsPerFrame.ToString() + " sim steps per frame";
				lastTime = Time.time;

				averageWalkingSpeed.SetDisplay( kinesin.averageWalkingSpeed );
			}

			if (Input.GetKeyDown( KeyCode.A ))
			{
				Motor motor = kinesin.motors.Find( m => m.binding );
				if (motor != null)
				{
					motor.CancelTubulinBind();
				}
			}
			if (Input.GetKeyDown( KeyCode.R ))
			{
				Reset();
			}
			if (Input.GetKey( KeyCode.Period ))
			{
				ChangeTime( 0.9f );
			}
			if (Input.GetKey( KeyCode.Comma ))
			{
				ChangeTime( 1.1f );
			}
		}

		float minTimeMultiplier = 100f;
		float maxTimeMultiplier = 10000f;
		void ChangeTime (float delta)
		{
			float timeMultiplier = Mathf.Clamp( MolecularEnvironment.Instance.timeMultiplier * delta, minTimeMultiplier, maxTimeMultiplier );
			MolecularEnvironment.Instance.SetTime( timeMultiplier );
		}
	}
}