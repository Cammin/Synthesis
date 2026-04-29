using UnityEngine;

namespace Synthesis;

public class SynthesizerAudio : MonoBehaviour
{
    public FMODAsset sfxStart;
    public FMOD_CustomLoopingEmitter sfxLoop;
    public FMODAsset sfxEnd;
    public Transform sfxLocation;

    public void PlayLoop()
    {
        sfxLoop.Play();
    }
    
    public void StopLoop()
    {
        sfxLoop.Stop();
    }

    public void PlayStart()
    {
        Utils.PlayFMODAsset(sfxStart, sfxLocation);
    }

    public void PlayEnd()
    {
        Utils.PlayFMODAsset(sfxEnd, sfxLocation);
    }
}