using System;
using System.Collections.Generic;
using System.Text;

namespace Geosoft.OpenGX.UtilityForms
{
	/// <summary>
	/// Interface allowing for a class to be requirable or not.
	/// </summary>
	public interface IRequirable
	{
		bool Required { get; set; }
		event EventHandler RequiredChanged;
	}
}
