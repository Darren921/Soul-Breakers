using System;
using Unity.VisualScripting;
using UnityEngine;
using Slider = UnityEngine.UI.Slider;

public class VolumeSlider : MonoBehaviour,IVolumeSlider
{
  [field:SerializeField]  public IVolumeSlider.SoundType _SoundType { get; set; }
    public Slider _VolumeSlider { get; set; }

    private void OnEnable()
    {
        _VolumeSlider = GetComponentInChildren<Slider>();
        SoundManager.instance.SoundVolumeUpdateAction += UpdateSliders;
        UpdateSliders();
    }

    private void OnDestroy()
    {
        SoundManager.instance.SoundVolumeUpdateAction -= UpdateSliders;
    }

    public void UpdateSliders()
    {
        switch (_SoundType)
        {
            case IVolumeSlider.SoundType.Master:
                _VolumeSlider.value = SoundManager.instance.soundData.CurrentVolume.curMasterVolume;
                break;
            case IVolumeSlider.SoundType.Sfx:
                _VolumeSlider.value = SoundManager.instance.soundData.CurrentVolume.curSFXVolume;
                break;
            case IVolumeSlider.SoundType.Music:
                _VolumeSlider.value = SoundManager.instance.soundData.CurrentVolume.curMusicVolume;
                break;
            case IVolumeSlider.SoundType.Interface:
                _VolumeSlider.value = SoundManager.instance.soundData.CurrentVolume.curInterfaceVolume;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

 public void OnSliderValueChanged()
    {
        switch (_SoundType)
        {
            case IVolumeSlider.SoundType.Master:
                 SoundManager.instance.soundData.CurrentVolume.curMasterVolume = _VolumeSlider.value ;
                break;
            case IVolumeSlider.SoundType.Sfx:
                SoundManager.instance.soundData.CurrentVolume.curSFXVolume = _VolumeSlider.value ;
                break;
            case IVolumeSlider.SoundType.Music:
                SoundManager.instance.soundData.CurrentVolume.curMusicVolume = _VolumeSlider.value ;
                break;
            case IVolumeSlider.SoundType.Interface:
                SoundManager.instance.soundData.CurrentVolume.curInterfaceVolume = _VolumeSlider.value ;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        SoundManager.instance.SoundVolumeUpdateAction?.Invoke();
    }
    
}
