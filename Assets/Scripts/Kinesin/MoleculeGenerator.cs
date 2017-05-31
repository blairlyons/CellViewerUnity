﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AICS.Kinesin
{
	public class MoleculeGenerator : MonoBehaviour 
	{
		[Tooltip("radius in nm")]
		public float generationRadius = 10f;
		[Tooltip("concentration in mM")]
		public float concentration = 5f;
		public Molecule moleculePrefab;

		List<Molecule> molecules = new List<Molecule>();

		int _number = -1;
		int number
		{
			get {
				if (_number < 0)
				{
					_number = Mathf.RoundToInt( 4f / 3f * Mathf.PI * concentration * Mathf.Pow( generationRadius, 3f ) * 1E-4f );
				}
				return _number;
			}
		}

		void Update ()
		{
			CheckBounds();
			CheckConcentration();
		}

		void CheckBounds ()
		{
			for (int i = 0; i < molecules.Count; i++)
			{
				if (molecules[i] != null)
				{
					if (Vector3.Distance( molecules[i].transform.position, transform.position ) > generationRadius)
					{
						molecules[i].shouldDestroy = true;
					}
				}
			}
			molecules.RemoveAll( molecule => molecule.shouldDestroy );
		}

		void CheckConcentration ()
		{
			if (moleculePrefab == null)
			{
				Debug.LogWarning("Molecule prefab is missing!");
				return;
			}

			int n = molecules.Count;
			while (n < number)
			{
				AddMolecule();
				n++;
			}
		}

		void AddMolecule ()
		{
			Vector3 position = transform.position + generationRadius * Random.insideUnitSphere;
			Molecule molecule = Instantiate( moleculePrefab, position, Random.rotation ) as Molecule;
			molecules.Add( molecule );
		}

		void OnDrawGizmos ()
		{
			Gizmos.DrawWireSphere( transform.position, generationRadius );
		}
	}
}
