using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Checkpoints : NetworkBehaviour
{
	private List<JankyCarControl_Multiplayer> crossedCarsList = new List<JankyCarControl_Multiplayer>();

	private int maxLapPoints;
	private int lapPoints;
	private int totalPlayers;
	[SerializeField] private Checkpoints previousCheckpoint;

	public bool isFirstCheckpoint;
	public bool isLastCheckpoint;

	private List<Checkpoints> checkpoints = new List<Checkpoints>();

	private void Start()
	{
		CheckpointManager.instance.AddCheckPoint(this);
	}
	public List<JankyCarControl_Multiplayer> GetCrossedCarsList()
	{
		return crossedCarsList;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other != null)
		{
			if (other.gameObject.TryGetComponent<JankyCarControl_Multiplayer>(out JankyCarControl_Multiplayer crossedCar))
			{
				Debug.Log($"{other.gameObject.name} triggered");
				if (!crossedCarsList.Contains(crossedCar))
				{
					if (crossedCarsList.Count < CheckpointManager.instance.GetTotalPlayerCount())
					{
						if (!isFirstCheckpoint)
						{
							crossedCarsList.Add(crossedCar);
							return;
						}

						else if (previousCheckpoint.GetCrossedCarsList().Contains(crossedCar))
						{
							crossedCarsList.Add(crossedCar);

							if (isLastCheckpoint)
							{
								crossedCar.FinishCurrentLap(lapPoints);

								if (lapPoints >= CheckpointManager.instance.pointsPerLap)
								{
									lapPoints -= CheckpointManager.instance.pointsPerLap;
								}



								if (crossedCarsList.Count == CheckpointManager.instance.GetTotalPlayerCount() - 1)
								{
									CheckpointManager.instance.LapFinished();

									lapPoints = maxLapPoints;

								}
							}



						}

					}


				}
			}
		}
	}

	public void SetLapPoints(int _maxLapPoints)
	{
		lapPoints = _maxLapPoints;

		maxLapPoints = _maxLapPoints;
	}
}
