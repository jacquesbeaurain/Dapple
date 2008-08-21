using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EARTHLib;

namespace GED.App.Core
{
	static class GoogleEarth
	{
		private static ApplicationGEClass s_oGEClass;

		public static ApplicationGEClass Instance
		{
			get { return s_oGEClass; }
		}

		public static void Init()
		{
			s_oGEClass = new ApplicationGEClass();
		}
	}
}
