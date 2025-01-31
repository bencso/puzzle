using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;


    private void Start()
    {
        if (musicSlider == null)
        {
            LoadMenuMusic();
        }
        else
        {
            if (PlayerPrefs.HasKey("musicVolume"))
            {
                LoadVolume();
            }
            else
            {
                SetMusicVolume();
                SetSFXVolume();
            }
        }
    }

    private void LoadMenuMusic()
    {
        float volume = PlayerPrefs.GetFloat("musicVolume");
        audioMixer.SetFloat("music", Mathf.Log10(volume) * 20f);
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        audioMixer.SetFloat("music", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        audioMixer.SetFloat("sfx", Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

        SetMusicVolume();
        SetSFXVolume();
    }
}
