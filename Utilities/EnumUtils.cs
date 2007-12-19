using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Utility
{
	public class EnumUtils
	{
		/// <summary>
		/// Turn an enum into its Description attribute text (if present) or its ToString value (if not).
		/// Referenced from http://blogs.msdn.com/abhinaba/archive/2005/10/20/483000.aspx
		/// </summary>
		/// <param name="eEnum">The enum to describe.</param>
		/// <returns>The description obtained.</returns>
		public static String GetDescription(Enum eEnum)
		{
			MemberInfo[] oMemInfos = eEnum.GetType().GetMember(eEnum.ToString());

			if (oMemInfos != null && oMemInfos.Length > 0)
			{
				Object[] oAttrs = oMemInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (oAttrs != null && oAttrs.Length > 0)
				{
					return ((DescriptionAttribute)oAttrs[0]).Description;
				}
			}

			return eEnum.ToString();
		}
	}
}
