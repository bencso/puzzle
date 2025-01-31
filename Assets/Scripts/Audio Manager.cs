using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    public AudioClip victory;
    public AudioClip succes;
    public AudioClip unsuccess;
    public AudioClip tilePlace1;
    public AudioClip tilePlace2;
    public AudioClip button;
    public AudioClip delete;


    private void Awake()
    {
        Instance = this;
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayButton()
    {
        sfxSource.PlayOneShot(button);
    }

    public void PlayDelete()
    {
        sfxSource.PlayOneShot(delete);
    }

    public void PlaySuccess()
    {
        sfxSource.PlayOneShot(succes);
    }

    public void PlayUnsuccess()
    {
        sfxSource.PlayOneShot(unsuccess);
    }

    public void PlayVictory()
    {
        sfxSource.PlayOneShot(victory);
    }

    public void PlayRandomTileSound()
    {
        int random = Random.Range(0, 2);

        switch (random)
        {
            case 0:
                sfxSource.PlayOneShot(tilePlace1);
                break;

            case 1:
                sfxSource.PlayOneShot(tilePlace2);
                break;
        }
    }
}
