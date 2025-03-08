using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class LapSystem : NetworkBehaviour
{
	public int totalLaps = 3; // Set in Inspector
	public int totalPoints = 30000; // Set in Inspector
	public TextMeshProUGUI lapCounterText;

	public void SetTotalPoints(int totalPlayerCount)
	{

	}



}
