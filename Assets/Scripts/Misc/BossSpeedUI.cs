using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossSpeedUI : MonoBehaviour
{
    public TMP_InputField speedInputField;

    private void Start()
    {
        if (BossSpeedModifier.Instance != null)
        {
            speedInputField.text = BossSpeedModifier.Instance.GetBossSpeed().ToString("F1");
        }
    }

    public void ApplySpeed()
    {
        if (BossSpeedModifier.Instance != null)
        {
            BossSpeedModifier.Instance.SetBossSpeed(speedInputField.text);
        }
    }
}
