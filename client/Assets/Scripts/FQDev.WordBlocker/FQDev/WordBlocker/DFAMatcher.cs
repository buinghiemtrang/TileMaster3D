using System.Collections.Generic;

namespace FQDev.WordBlocker
{
	public class DFAMatcher
	{
		private class Record
		{
			public readonly Dictionary<char, Record> SubCharacters = new Dictionary<char, Record>();

			public bool IsEnd;
		}

		public struct Result
		{
			public int StartIndex;

			public int EndIndex;
		}

		private readonly Dictionary<char, Record> _charSet = new Dictionary<char, Record>();

		private int _matchStartIndex;

		private int _matchEndIndex;

		public void AddWords(IEnumerable<string> words)
		{
			foreach (string word in words)
			{
				// [rebuild] literal xác minh bằng disasm 0x16a5348: "^ *$" (bỏ dòng rỗng/toàn dấu cách)
				if (!System.Text.RegularExpressions.Regex.IsMatch(word, "^ *$"))
				{
					AddWord(word, _charSet);
				}
			}
		}

		private static void AddWord(string word, Dictionary<char, Record> set)
		{
			Record value = null;
			if (string.IsNullOrEmpty(word))
			{
				return;
			}
			char key = word[0];
			int num = 1;
			while (true)
			{
				if (!set.TryGetValue(key, out value))
				{
					value = new Record();
					set.Add(key, value);
				}
				if (word.Length - 1 <= num - 1)
				{
					break;
				}
				set = value.SubCharacters;
				key = word[num];
				num++;
			}
			value.IsEnd = true;
		}

		public bool MatchWord(string word, ICollection<Result> resultReceiver = null)
		{
			resultReceiver?.Clear();
			bool result = false;
			int num = 0;
			while (num < word.Length)
			{
				while (FindMatch(ref word, num))
				{
					if (resultReceiver == null)
					{
						return true;
					}
					resultReceiver.Add(new Result
					{
						StartIndex = _matchStartIndex,
						EndIndex = _matchEndIndex
					});
					result = true;
					num = _matchEndIndex + 1;
					if (num >= word.Length)
					{
						return true;
					}
				}
				if (_matchStartIndex > 0)
				{
					num = _matchStartIndex;
				}
				num++;
			}
			return result;
		}

		private bool FindMatch(ref string word, int startIndex)
		{
			Record value = null;
			_matchStartIndex = -1;
			_matchEndIndex = -1;
			while (startIndex < word.Length)
			{
				if (_charSet.TryGetValue(word[startIndex], out value))
				{
					_matchStartIndex = startIndex;
					FindTail(ref word, startIndex, value);
					if (_matchStartIndex > -1)
					{
						return _matchEndIndex >= 0;
					}
					return false;
				}
				startIndex++;
			}
			return false;
		}

		private void FindTail(ref string word, int index, Record record)
		{
			Record value = null;
			while (true)
			{
				if (record.IsEnd)
				{
					_matchEndIndex = index;
				}
				if (index == word.Length - 1)
				{
					break;
				}
				index++;
				if (!record.SubCharacters.TryGetValue(word[index], out value))
				{
					break;
				}
				record = value;
			}
		}
	}
}
