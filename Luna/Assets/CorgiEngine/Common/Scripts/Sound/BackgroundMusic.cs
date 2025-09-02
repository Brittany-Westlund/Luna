using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class to a GameObject to have it play a background music when instantiated.
	/// Careful: only one background music will be played at a time.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Audio/Background Music")]
	public class BackgroundMusic : MMPersistentHumbleSingleton<BackgroundMusic>
	{
		[Tooltip("the background music audio clip to play")]
		public AudioClip SoundClip;
		
		[Tooltip("whether or not the music should loop")]
		public bool Loop = true;
		
		[Tooltip("the ID to create this background music with")]
		public int ID = 255;
		
		[Tooltip("the volume of the background music")]
		[Range(0f, 1f)]
		public float Volume = 1f;  // Volume control, with a default of 1 (full volume)

		protected AudioSource _source;

		/// <summary>
		/// Statics initialization to support enter play modes
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

		/// <summary>
		/// Gets the AudioSource associated with that GameObject and asks the GameManager to play it with specified options.
		/// </summary>
		protected virtual void Start()
		{
			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.ID = ID;
			options.Loop = Loop;
			options.Location = Vector3.zero;
			options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
			options.Volume = Volume;  // Set the volume from the public Volume field
            
			MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
		}
	}
}
