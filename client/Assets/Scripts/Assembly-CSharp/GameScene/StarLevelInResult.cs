namespace GameScene
{
	public class StarLevelInResult : ActivityInResult
	{
		public override bool IsOpen()
		{
			return true;
		}

		public override int GetScore()
		{
			return DB.User.SumStarLevel;
		}
	}
}
