using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioGroupHelperBase : MonoBehaviour, IAudioGroupHelper
{
	public virtual AudioMixerGroup AudioMixerGroup{ get; set; }
}

