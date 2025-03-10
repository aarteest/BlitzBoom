using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

public class RaceCountdown : NetworkBehaviour
{
    public TMP_Text countdownText; // Assign in the UI
    private float countdownTime = 3f;
    private NetworkVariable<int> countdownSync = new NetworkVariable<int>(3, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(Countdown());
        }

        countdownSync.OnValueChanged += OnCountdownChanged;
    }

    private IEnumerator Countdown()
    {
        while (countdownSync.Value > 0)
        {
            countdownSync.Value--; // Sync countdown across clients
            yield return new WaitForSeconds(1f);
        }

        countdownSync.Value = -1; // Show "GO!"
        yield return new WaitForSeconds(1f);

        HideCountdownClientRpc();
    }

    private void OnCountdownChanged(int oldValue, int newValue)
    {
        if (newValue > 0)
        {
            StartCoroutine(AnimateCountdown(newValue.ToString())); // Animate each number
        }
        else if (newValue == -1)
        {
            StartCoroutine(AnimateCountdown("GO!"));
        }
    }

    private IEnumerator AnimateCountdown(string text)
    {
        countdownText.text = text;
        countdownText.color = new Color(1, 1, 1, 0); // Start fully transparent

        float duration = 0.5f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            countdownText.color = new Color(1, 1, 1, timer / duration); // Fade in
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // Pause fully visible

        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            countdownText.color = new Color(1, 1, 1, 1 - (timer / duration)); // Fade out
            yield return null;
        }
    }

    [ClientRpc]
    private void HideCountdownClientRpc()
    {
        countdownText.enabled = false;  // Hide the countdown for all players
    }
}








//using Unity.Netcode;
//using UnityEngine;
//using TMPro;
//using System.Collections;

//public class RaceCountdown : NetworkBehaviour
//{
//    public TMP_Text countdownText; // Assign in the UI
//    private float countdownTime = 3f;

//    private NetworkVariable<float> countdownSync = new NetworkVariable<float>();

//    private void Start()
//    {
//        if (!IsServer) return;

//        if (IsServer)
//        {
//            countdownSync.Value = countdownTime; // Sync the countdown value to the clients
//            StartCoroutine(Countdown());
//        }
//    }

//    private IEnumerator Countdown()
//    {
//        float countdownDuration = countdownSync.Value;  // Starting countdown value
//        Color startColor = Color.red;
//        Color endColor = Color.green;
//        float transitionDuration = 4f;  // Duration for color transition

//        while (countdownSync.Value > 0)
//        {
//            countdownSync.Value = countdownSync.Value; // Sync to clients immediately
//            countdownText.text = countdownSync.Value.ToString("F0");

//            // Gradually change the color over time
//            float lerpTime = Mathf.Clamp01((countdownDuration - countdownSync.Value) / transitionDuration);
//            countdownText.color = Color.Lerp(startColor, endColor, lerpTime);

//            yield return new WaitForSeconds(1f);
//            countdownSync.Value--; // Decrement the countdown
//        }

//        countdownText.text = "GO!";
//        countdownSync.Value = -1; // Mark that the countdown is over (GO!)

//        // Make sure the color transition is finished
//        countdownText.color = endColor;

//        // Call ClientRpc to hide the text on all clients
//        HideCountdownClientRpc();

//        yield return new WaitForSeconds(1f);
//    }

//    [ClientRpc]
//    private void HideCountdownClientRpc()
//    {
//        countdownText.enabled = false;  // Hide the countdown for all players
//    }

//    private void Update()
//    {
//        if (countdownSync.Value > 0)
//        {
//            countdownText.text = countdownSync.Value.ToString("F0");
//        }
//        else if (countdownSync.Value == -1)
//        {
//            countdownText.text = "GO!";
//        }
//    }
//}


////////////////////////////////////////////////////////////////////////////////////////////////////////////



//using Unity.Netcode;
//using UnityEngine;
//using TMPro;
//using System.Collections;

//public class RaceCountdown : NetworkBehaviour
//{
//    public TMP_Text countdownText; // Assign in the UI
//    private float countdownTime = 3f;

//    private NetworkVariable<float> countdownSync = new NetworkVariable<float>();

//    private void Start()
//    {
//        if (IsServer)
//        {
//            countdownSync.Value = countdownTime; // Sync the countdown value to the clients
//            StartCoroutine(Countdown());
//        }
//    }

//    private IEnumerator Countdown()
//    {
//        float countdownDuration = countdownSync.Value;  // Starting countdown value
//        Color startColor = Color.red;
//        Color endColor = Color.green;
//        float transitionDuration = 4f;  // Duration for color transition

//        while (countdownSync.Value > 0)
//        {
//            countdownSync.Value = countdownSync.Value; // Sync to clients immediately
//            countdownText.text = countdownSync.Value.ToString("F0");

//            // Gradually change the color over time
//            float lerpTime = Mathf.Clamp01((countdownDuration - countdownSync.Value) / transitionDuration);
//            countdownText.color = Color.Lerp(startColor, endColor, lerpTime);

//            yield return new WaitForSeconds(1f);
//            countdownSync.Value--; // Decrement the countdown
//        }

//        countdownText.text = "GO!";
//        countdownSync.Value = -1; // Mark that the countdown is over (GO!)

//        // Make sure the color transition is finished
//        countdownText.color = endColor;

//        yield return new WaitForSeconds(1f);
//        countdownText.enabled = false;  // Set text to inactive at the end
//    }




//    //private IEnumerator Countdown()
//    //{
//    //    while (countdownSync.Value > 0)
//    //    {
//    //        countdownSync.Value = countdownSync.Value; // Sync to clients immediately
//    //        countdownText.text = countdownSync.Value.ToString("F0");
//    //        yield return new WaitForSeconds(1f);
//    //        countdownSync.Value--; // Decrement the countdown
//    //    }

//    //    countdownText.text = "GO!";
//    //    countdownSync.Value = -1; // Mark that the countdown is over (GO!)
//    //    yield return new WaitForSeconds(1f);
//    //    countdownText.enabled = false;
//    //}

//    // Listen for updates to countdownSync from the server and update the text for clients
//    private void Update()
//    {
//        if (!IsServer && countdownSync.Value != -1)
//        {
//            countdownText.text = countdownSync.Value.ToString("F0");
//        }
//        else if (countdownSync.Value == -1)
//        {
//            countdownText.text = "GO!";
//        }
//    }
//}





//using Unity.Netcode;
//using UnityEngine;
//using TMPro;
//using System.Collections;

//public class RaceCountdown : NetworkBehaviour
//{
//    public TMP_Text countdownText; // Assign in the UI
//    private float countdownTime = 3f;

//    private void Start()
//    {
//        if (IsServer)
//        {
//            StartCoroutine(Countdown());
//        }
//    }

//    private IEnumerator Countdown()
//    {
//        while (countdownTime > 0)
//        {
//            countdownText.text = countdownTime.ToString("F0");
//            UpdateCountdownClientRpc(countdownText.text);
//            yield return new WaitForSeconds(1f);
//            countdownTime--;
//        }

//        countdownText.text = "GO!";
//        UpdateCountdownClientRpc("GO!");
//        yield return new WaitForSeconds(1f);
//        countdownText.enabled = false;
//    }

//    [ClientRpc]
//    private void UpdateCountdownClientRpc(string countdownValue)
//    {
//        countdownText.text = countdownValue;
//    }
//}