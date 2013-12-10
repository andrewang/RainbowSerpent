using System;
using System.Collections.Generic;

namespace SerpentExtensions
{
	public static class Extensions
	{
		static public string GetString(this Dictionary<string,object> dict, string key)
		{
			if (dict.ContainsKey(key) == false)	{ return null; }
			object o = dict[key];
			if (!(o is String)) {return null; }

			return o as String;
		}

		static public int GetInt(this Dictionary<string,object> dict, string key)
		{
			if (dict.ContainsKey(key) == false)	{ return 0; }
			object o = dict[key];
			if (o is int) 
			{
				return (int) o; 
			}
			if (o is long)
			{
				long longInt = (long) o;
				int i = (int) longInt;
				return i;
			}

			return 0;
		}

		static public object GetObject(this Dictionary<string,object> dict, string key)
		{
			if (dict.ContainsKey(key) == false)	{ return null; }
			object o = dict[key];
			return o;
		}
	}

}