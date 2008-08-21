using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace GED.WebService
{
	public static class ControlPanel
	{
		public const int DefaultPort = 39277;

		static ServiceHost s_oHost;
		static Uri s_oBaseAddress;

		public static void Start()
		{
			Start(DefaultPort);
		}

		public static void Start(int iPort)
		{
			#region // Input and state checking
			if (s_oHost != null) throw new InvalidOperationException("Web service is already started.");
			#endregion

			s_oBaseAddress = new Uri(String.Format("http://localhost:{0}", iPort));
			s_oHost = new ServiceHost(typeof(Implementation), s_oBaseAddress);

			ServiceEndpoint oEndpoint = s_oHost.AddServiceEndpoint(typeof(Contract), new WebHttpBinding(), String.Empty);
			oEndpoint.Behaviors.Add(new WebHttpBehavior());

			s_oHost.Open();
		}

		public static Uri BaseAddress
		{
			get { return s_oBaseAddress; }
		}

		public static void Stop()
		{
			#region // Input and state checking
			#endregion

			if (s_oHost != null)
			{
				s_oHost.Close();
				s_oHost = null;
			}
		}
	}
}
