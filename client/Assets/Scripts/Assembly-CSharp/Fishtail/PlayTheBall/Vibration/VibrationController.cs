using MoreMountains.NiceVibrations;
using UnityEngine;

namespace Fishtail.PlayTheBall.Vibration
{
	public class VibrationController : MonoBehaviour
	{
		private static VibrationController _instance;

		public static VibrationController I => _instance;

		public bool IsShockdOn { get; private set; }

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
				Object.DontDestroyOnLoad(base.gameObject);
			}
			IsShockdOn = StorageMgr.GetBool("User.bVibrate", true);
			MMVibrationManager.iOSInitializeHaptics();
		}

		private void OnDestroy()
		{
			MMVibrationManager.iOSReleaseHaptics();
		}

		public void SetShockOn(bool b_open)
		{
			if (IsShockdOn != b_open)
			{
				IsShockdOn = b_open;
			}
		}

		public void Impact()
		{
			if (IsShockdOn)
			{
				MMVibrationManager.Haptic(HapticTypes.MediumImpact);
			}
		}

		public void ImpactL()
		{
			if (IsShockdOn)
			{
				MMVibrationManager.Haptic(HapticTypes.LightImpact);
			}
		}

		public void ImpactM()
		{
			if (IsShockdOn)
			{
				MMVibrationManager.Haptic(HapticTypes.MediumImpact);
			}
		}

		public void ImpactH()
		{
			if (IsShockdOn)
			{
				MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
			}
		}
	}
}
