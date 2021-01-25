using System;
using System.Linq;
using System.Reflection;

namespace O10.Core.ExtensionMethods
{
	public static class EnumExtensionMethods
	{
		public static string GetDescription(this Enum genericEnum)
		{
			Type genericEnumType = genericEnum.GetType();
			MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());
			if ((memberInfo != null && memberInfo.Length > 0))
			{
				var attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
				if ((attrs != null && attrs.Count() > 0))
				{
					return ((System.ComponentModel.DescriptionAttribute)attrs.ElementAt(0)).Description;
				}
			}
			return genericEnum.ToString();
		}

		public static string GetCategory(this Enum genericEnum)
		{
			Type genericEnumType = genericEnum.GetType();
			MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());
			if ((memberInfo != null && memberInfo.Length > 0))
			{
				var attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.CategoryAttribute), false);
				if ((attrs != null && attrs.Count() > 0))
				{
					return ((System.ComponentModel.CategoryAttribute)attrs.ElementAt(0)).Category;
				}
			}
			return string.Empty;
		}
	}
}
