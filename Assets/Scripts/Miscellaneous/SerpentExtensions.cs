using System;
using System.Collections.Generic;

namespace SerpentExtensions
{
	public static class Extensions
	{
		/// <summary>
		/// Gets a string contained in a dictionary with the given key
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="dict">Dict.</param>
		/// <param name="key">Key.</param>
		static public string GetString(this Dictionary<string,object> dict, string key)
		{
			if (dict.ContainsKey(key) == false)	{ return null; }
			object o = dict[key];
			if (!(o is String)) {return null; }

			return o as String;
		}
		
		/// <summary>
		/// Gets an int contained in a dictionary with the given key		
		/// </summary>
		/// <returns>The int.</returns>
		/// <param name="dict">Dict.</param>
		/// <param name="key">Key.</param>
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

		/// <summary>
		/// Gets an object from a dictionary with a given key
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="dict">Dict.</param>
		/// <param name="key">Key.</param>
		static public object GetObject(this Dictionary<string,object> dict, string key)
		{
			if (dict.ContainsKey(key) == false)	{ return null; }
			object o = dict[key];
			return o;
		}
	}

}