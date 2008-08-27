using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace GED.WebService
{
	/// <summary>
	/// Provides static methos for other modules to use the GED web service.
	/// </summary>
	public static class ControlPanel
	{
		#region Constants

		/// <summary>
		/// The port that the web service will listen on if no port is specified in the Start() method.
		/// </summary>
		public const int DefaultPort = 39277;

		#endregion


		#region Member Variables

		static ServiceHost s_oHost;
		static Uri s_oBaseAddress;

		#endregion


		#region Public Methods

		/// <summary>
		/// Starts the GED web service listening on the default port.
		/// </summary>
		public static void Start()
		{
			Start(DefaultPort);
		}

		/// <summary>
		/// Starts the GED web service.
		/// </summary>
		/// <param name="iPort">The port that the GED web service will listen on.</param>
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

		/// <summary>
		/// Stop the GED web service.
		/// </summary>
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

		#endregion


		#region Properties

		/// <summary>
		/// The base address of the GED web service.
		/// </summary>
		public static Uri BaseAddress
		{
			get { return s_oBaseAddress; }
		}

		#endregion
	}
}
