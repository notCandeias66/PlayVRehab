using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] public Slider volumeSlider;
    public MusicManager musicManager;

    void Start()
    {
        if (volumeSlider != null && musicManager != null)
        {
            volumeSlider.onValueChanged.AddListener(UpdateVolume);
            volumeSlider.value = musicManager.GetVolume() * 100;
        }
    }

    public void UpdateVolume(float value)
    {
        musicManager.SetVolume(value / 100f);
    }
}
