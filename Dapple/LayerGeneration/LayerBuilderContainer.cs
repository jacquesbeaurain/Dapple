using System;
using System.Collections.Generic;
using System.Text;
using WorldWind;
using System.Windows.Forms;
using Geosoft.DotNetTools;
using WorldWind.PluginEngine;
using Dapple;
using Dapple.LayerGeneration;

namespace Dapple.LayerGeneration
{
	public class LayerBuilderContainer
	{
		private LayerBuilder m_builder;
		private LayerBuilder m_buildersource;
		private string m_name;
		private string m_uri;
		private bool m_visible;
		private bool m_temporary;
      private byte m_opacity;

		public LayerBuilderContainer(string strName, string strUri, bool visible, byte opacity)
		{
			m_name = strName;
			m_uri = strUri;
			m_visible = visible;
			m_opacity = opacity;
		}

		public LayerBuilderContainer(LayerBuilder builder)
			: this(builder, true)
		{
		}

		public LayerBuilderContainer(LayerBuilder builder, bool bClone)
		{
			m_buildersource = builder;
			if (bClone)
				m_builder = builder.Clone() as LayerBuilder;
			else
				m_builder = builder;
			m_uri = m_builder.GetURI();
			m_visible = m_builder.Visible;
			m_opacity = m_builder.Opacity;
         m_temporary = m_builder.Temporary;
			m_name = m_builder.Name;
		}

		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		public string Uri
		{
			get
			{
				if (m_builder == null)
					return m_uri;
				else
					return m_builder.GetURI();
			}
		}

		public bool Visible
		{
			get
			{
				if (m_builder == null)
					return m_visible;
				else
					return m_builder.Visible;
			}
			set
			{
				m_visible = value;
				if (m_builder != null && m_builder.Visible != value)
					m_builder.Visible = value;
			}
		}


		public byte Opacity
		{
			get
			{
				if (m_builder == null)
					return m_opacity;
				else
					return m_builder.Opacity;
			}
			set
			{
				m_opacity = value;
				if (m_builder != null && m_builder.Opacity != value)
					m_builder.Opacity = value;
			}
		}

		public bool Temporary
		{
			get
			{
				return m_temporary;
			}
			set
			{
				m_temporary = value;
            m_builder.Temporary = value;
			}
		}

		public LayerBuilder Builder
		{
			get
			{
				return m_builder;
			}
		}

		public LayerBuilder SourceBuilder
		{
			get
			{
				return m_buildersource;
			}
			set
			{
				m_buildersource = value;
				if (m_buildersource != null)
				{
					m_builder = m_buildersource.CloneSpecific() as LayerBuilder;
					m_builder.Opacity = m_opacity;
					m_builder.Visible = m_visible;
				}
				else
					m_builder = null;
			}
		}
	}
}
