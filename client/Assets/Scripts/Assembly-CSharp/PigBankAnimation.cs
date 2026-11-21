public class PigBankAnimation : MainIconAnimation
{
	public override void SetInfo()
	{
		if (PigBankData.Instance.NeedDoAnimation)
		{
			_newAddKeyCount = pig_const.PigReward;
			PigBankData.Instance.NeedDoAnimation = false;
		}
	}
}
