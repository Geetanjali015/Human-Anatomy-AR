
using UnityEngine;
using TMPro;

public class BodySystemLabel : MonoBehaviour
{
    public TMP_Text systemNameLabel;

    public void ShowAnthrology()
    {
        systemNameLabel.text = "Anthrology";
    }

    public void ShowMuscular()
    {
        systemNameLabel.text = "Muscular System";
    }

    public void ShowAngiology()
    {
        systemNameLabel.text = "Angiology";
    }

    public void ShowNeurology()
    {
        systemNameLabel.text = "Neurology";
    }

    public void ShowSplanchnology()
    {
        systemNameLabel.text = "Splanchnology";
    }

    public void ShowMyology()
    {
        systemNameLabel.text = "Myology";
    }
}
