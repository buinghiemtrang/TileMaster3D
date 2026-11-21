using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FQDev.WordBlocker
{
	public class TextFilter
	{
		private DFAMatcher _matcher = new DFAMatcher();

		private List<DFAMatcher.Result> _resultReceiver = new List<DFAMatcher.Result>();

		public void LoadKeyWords(IEnumerable<string> keywords)
		{
			_matcher.AddWords(keywords);
		}

		public bool CheckWord(string word)
		{
			return _matcher.MatchWord(word);
		}

		public string AlterWord(string word, char replacement)
		{
			string text = word;
			if (!IsComboCode(word))
			{
				_matcher.MatchWord(word, _resultReceiver);
				foreach (DFAMatcher.Result item in _resultReceiver)
				{
					int num = item.EndIndex - item.StartIndex;
					string oldValue = word.Substring(item.StartIndex, num + 1);
					string newValue = new string(replacement, num + 1);
					text = text.Replace(oldValue, newValue);
				}
			}
			return text;
		}

		private static bool IsComboCode(string text)
		{
			return Regex.IsMatch(text, "\\[(.*?)\\:(.*?)\\]");
		}

		public static bool ContainInvalidSymbols(string text)
		{
			return !Regex.IsMatch(text, "^[\u4e00-\u9fa5A-Za-z]+$");
		}
	}
}
