using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FQDev.UserLabel;

public class UserLabelManager
{
	private readonly Dictionary<string, string> _fieldLabels = new Dictionary<string, string>();

	private UserRule _matchedRule;

	private UserLabelConfig _config;

	public static UserLabelManager Instance { get; } = new UserLabelManager();

	public IReadOnlyDictionary<string, string> FieldLabels => _fieldLabels;

	public UserRule MatchedRule => _matchedRule;

	public UserLabelConfig Config => _config;

	public void SetConfig(UserLabelConfig config)
	{
		_config = config;
	}

	public void FetchLabels(IEnumerable<MethodInfo> methodInfos)
	{
		foreach (MethodInfo methodInfo in methodInfos)
		{
			UserLabelGetterAttribute customAttribute = methodInfo.GetCustomAttribute<UserLabelGetterAttribute>();
			if (customAttribute != null)
			{
				_fieldLabels[customAttribute.Field] = methodInfo.Invoke(null, null) as string;
			}
		}
	}

	public void RefreshMatchedRule()
	{
		if (_config == null)
		{
			return;
		}
		List<UserGroup> list = _config.Groups.FindAll(IsMatchWith);
		_matchedRule = null;
		if (list.Count != 0)
		{
			UserGroup userGroup = list[0];
			if (list.Count > 1)
			{
				list.Sort((UserGroup a, UserGroup b) => a.Priority - b.Priority);
				userGroup = list[list.Count - 1];
			}
			if (_config.Rules.TryGetValue(userGroup.RuleName, out var value))
			{
				_matchedRule = value;
			}
		}
	}

	public string GetFieldValue(string field)
	{
		if (_config == null)
		{
			return string.Empty;
		}
		_fieldLabels.TryGetValue(field, out var value);
		return value;
	}

	public bool TryGetFilterByField(string field, out UserFilter filter)
	{
		filter = null;
		if (_config == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, UserFilter> item in _config.Filters)
		{
			if (!(item.Value.Field != field))
			{
				filter = item.Value;
				return true;
			}
		}
		return false;
	}

	public bool TryGetFilterByLabel(string label, out UserFilter filter)
	{
		filter = null;
		if (_config == null)
		{
			return false;
		}
		return _config.Filters.TryGetValue(label, out filter);
	}

	public void SetRule(string rule)
	{
		_matchedRule = null;
		if (_config != null)
		{
			_matchedRule = (_config.Rules.TryGetValue(rule, out var value) ? value : null);
		}
	}

	public bool IsMatchWith(UserGroup group)
	{
		if (_config == null)
		{
			return false;
		}
		if (group.FiltersRequired != null)
		{
			foreach (string item in group.FiltersRequired)
			{
				if (!_config.Filters.TryGetValue(item, out var value) || !_fieldLabels.TryGetValue(value.Field, out var value2) || !Compare(value2, value.Equation, value.Values))
				{
					return false;
				}
			}
		}
		if (group.FiltersExcluded != null)
		{
			foreach (string item2 in group.FiltersExcluded)
			{
				if (!_config.Filters.TryGetValue(item2, out var value3) || !_fieldLabels.TryGetValue(value3.Field, out var value4) || Compare(value4, value3.Equation, value3.Values))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool Compare(string fieldValue, EquationType equationType, List<string> values)
	{
		if (string.IsNullOrEmpty(fieldValue))
		{
			return false;
		}
		switch (equationType)
		{
		case EquationType.Full:
			return values.Contains(fieldValue);
		case EquationType.ContainTarget:
			return values.Any((string label) => fieldValue.IndexOf(label, StringComparison.InvariantCultureIgnoreCase) >= 0);
		case EquationType.ContainField:
			return values.Any((string label) => label.IndexOf(fieldValue, StringComparison.InvariantCultureIgnoreCase) >= 0);
		case EquationType.StartContainTarget:
			return values.Any(fieldValue.StartsWith);
		case EquationType.StartContainField:
			return values.Any((string label) => label.StartsWith(fieldValue));
		case EquationType.RangeInt:
		{
			if (values.Count == 2 && int.TryParse(fieldValue, out var result))
			{
				int result2 = int.MinValue;
				if (!values[0].Equals("min") && !int.TryParse(values[0], out result2))
				{
					return false;
				}
				int result3 = int.MaxValue;
				if (!values[1].Equals("max") && !int.TryParse(values[1], out result3))
				{
					return false;
				}
				if (result >= result2)
				{
					return result <= result3;
				}
				return false;
			}
			return false;
		}
		default:
			return false;
		}
	}

	public bool Compare(string field, UserFilter filter)
	{
		if (_config == null)
		{
			return false;
		}
		return Compare(field, filter.Equation, filter.Values);
	}

	public bool Compare(UserFilter filter)
	{
		if (_config == null)
		{
			return false;
		}
		if (_fieldLabels.TryGetValue(filter.Field, out var value))
		{
			return Compare(value, filter.Equation, filter.Values);
		}
		return false;
	}

	public void Clear()
	{
		_fieldLabels.Clear();
		_config = null;
	}

	public string GetRealPath(string origin)
	{
		if (_matchedRule != null && _matchedRule.Replacements.TryGetValue(origin, out var value))
		{
			return value;
		}
		return origin;
	}
}
