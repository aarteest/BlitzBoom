using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public SinglePlayerCarController carController; // Reference to the car script
    [SerializeField] private TMP_Text speedText; // Reference to the speedometer text

    void Update()
    {
        if (carController != null)
        {
            // Get the actual speed from the car controller
            float currentSpeed = carController.GetCurrentSpeed();
            UpdateSpeedometer(currentSpeed);
        }
    }

    private void UpdateSpeedometer(float speed)
    {
        speedText.text = $"{(int)speed} MPH"; // Display the speed as an integer
    }
}
