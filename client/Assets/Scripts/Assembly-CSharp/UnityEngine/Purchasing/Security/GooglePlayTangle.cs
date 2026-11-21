namespace UnityEngine.Purchasing.Security
{
	public class GooglePlayTangle
	{
		private static byte[] data = System.Convert.FromBase64String("vmGnDmqNb299c5wo+JEotbe85j2IajrSNwUwFbJR+5xwIim97dVUt2vNbheb89cdslDpEw059cmFzfcgEKIhAhAtJikKpmim1y0hISElICO4TML7Y3fFX7Tmlh/IRFxg7a4aP0q9o79chyJM5gudHpk+I0yD4UB27be6uFMTEQEuqcygA5zDM3G7jlrrU/znx4K+ZLD6Sz8dspQXYqVj8nu6NR2iACiCO5GRONgjTri9RmzKC4zj5r0/tTeJB3Mg/FfSjM2r3+I5D6bhz/M0AXA2dU99jXOz3NdgsqIhLyAQoiEqIqIhISCprIUZa1HmWREeKZBAHWHLvbQ6tXWJrtfiz7AbL99DGiklrKCs8s1YUsL59N2Mk5sk/BELyvit6SIjISAh");

		private static int[] order = new int[]
		{
			9, 13, 3, 9, 7, 11, 12, 7, 12, 9,
			10, 13, 12, 13, 14
		};

		private static int key = 32;

		public static readonly bool IsPopulated = true;

		public static byte[] Data()
		{
			if (!IsPopulated)
			{
				return null;
			}
			return Obfuscator.DeObfuscate(data, order, key);
		}
	}
}
