using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Dapple.LayerGeneration;
using System.ComponentModel;

namespace NewServerTree
{
	/// <summary>
	/// The current load status of a ModelNode.
	/// </summary>
	internal enum LoadState
	{
		Unloaded,
		Loading,
		LoadFailed,
		LoadSuccessful
	}


	/// <summary>
	/// The root of the ModelNode inheritance hierarchy.
	/// </summary>
	internal abstract class ModelNode : IComparable<ModelNode>
	{
		#region Constants

		/// <summary>
		/// Set to true to enable some ModelNodes to always display all of their children when
		/// open, regardless of whether they are the selected node or not.
		/// </summary>
		internal const bool UseShowAllChildren = false;

		protected const string ErrLoadedLeafNode = "Tried to load a leaf ModelNode";
		protected const string ErrLoadedBadNode = "Tried to load a ModelNode that doesn't load asynchronously";

		#endregion


		#region Events

		protected void OnDisplayInfoChanged()
		{
			m_oModel.ModelNodeDisplayUpdated(this);
		}

		private void OnLoaded()
		{
			m_oModel.ModelNodeLoaded(this);
		}

		private void OnUnloaded()
		{
			m_oModel.ModelNodeUnloaded(this);
		}

		#endregion


		#region Member Variables

		protected DappleModel m_oModel = null;
		private ModelNode m_oParent = null;
		private List<ModelNode> m_oChildren = new List<ModelNode>();

		#endregion


		#region Constructor

		internal ModelNode(DappleModel oModel)
		{
			m_oModel = oModel;
		}

		#endregion


		#region Properties

		/// <summary>
		/// Get the parent ModelNode of this ModelNode.
		/// </summary>
		[Browsable(false)]
		internal ModelNode Parent
		{
			get { return m_oParent; }
		}

		/// <summary>
		/// Whether this ModelNode is a leaf node (no children, don't collapse parent in 
		/// ServerTree when selected).
		/// </summary>
		[Browsable(false)]
		internal virtual bool IsLeaf
		{
			get { return false; }
		}

		/// <summary>
		/// Whether to disallow pruning of this ModelNode's child nodes in the ServerTree.
		/// </summary>
		[Browsable(false)]
		internal virtual bool ShowAllChildren
		{
			get { return false; }
		}

		/// <summary>
		/// Get a String describing this ModelNode.
		/// </summary>
		internal abstract String DisplayText { get; }

		/// <summary>
		/// Get a String annotating this ModelNode (such as the number of datasets in a server).
		/// </summary>
		internal virtual String Annotation
		{
			get { return String.Empty; }
		}

		/// <summary>
		/// The ImageKey of the TreeNode for this ModelNode.
		/// </summary>
		[Browsable(false)]
		internal abstract String IconKey { get; }

		#endregion


		#region Loading

		private delegate ModelNode[] LoadDelegate();
		private IAsyncResult m_oCurrentAsyncResult = null;
		private LoadState m_eStatus = LoadState.Unloaded;
		private Object m_oAsyncLock = new Object();
		private int m_iLoadSync = 0;

		private class AsyncContext
		{
			private int m_iLoadSync;
			private LoadDelegate m_oDelegate;

			internal AsyncContext(int iSyncNumber, LoadDelegate oDelegate)
			{
				m_iLoadSync = iSyncNumber;
				m_oDelegate = oDelegate;
			}

			internal int SyncNumber { get { return m_iLoadSync; } }
			internal LoadDelegate LoadDelegate { get { return m_oDelegate; } }
		}

		/// <summary>
		/// Gets the child ModelNodes of this ModelNode.
		/// </summary>
		/// <remarks>
		/// If this method will take a long time (say, because we have to do an internet connection
		/// to get the data), then the class should have LoadSynchronously return false.
		/// </remarks>
		/// <returns>A list of ModelNodes to add to this ModelNode.</returns>
		protected abstract ModelNode[] Load();

		internal void UnloadSilently()
		{
			m_oModel.DoWithLock(new MethodInvoker(_UnloadInModelLock));
		}

		/// <summary>
		/// Unloads this ModelNode: its children are cleared, and its LoadState is reset to Unloaded.
		/// </summary>
		internal void Unload()
		{
			UnloadSilently();
			OnUnloaded();
		}

		private void _UnloadInModelLock()
		{
			lock (m_oAsyncLock)
			{
				m_iLoadSync++;

				foreach (ModelNode oNode in m_oChildren)
				{
					oNode.m_oParent = null;
				}
				m_oChildren.Clear();

				m_eStatus = LoadState.Unloaded;
			}
		}

		/// <summary>
		/// Loads this ModelNode asynchronously.
		/// </summary>
		internal void BeginLoad()
		{
			lock (m_oAsyncLock)
			{
				LoadDelegate oLoad = new LoadDelegate(Load);
				AsyncContext oContext = new AsyncContext(m_iLoadSync, oLoad);
				m_oCurrentAsyncResult = oLoad.BeginInvoke(_EndLoad, oContext);
				m_eStatus = LoadState.Loading;
			}
		}

		private void _EndLoad(IAsyncResult oResult)
		{
			m_oModel.DoWithLock(new WaitCallback(_EndLoadInModelLock), oResult);
			OnDisplayInfoChanged();
			OnLoaded();
		}

		private void _EndLoadInModelLock(Object oParams)
		{
			IAsyncResult oResult = oParams as IAsyncResult;
			AsyncContext oContext = oResult.AsyncState as AsyncContext;

			lock (m_oAsyncLock)
			{
				if (oContext.SyncNumber != m_iLoadSync)
				{
					return;
				}
			}

			ModelNode[] oChildren = null;
			try
			{
				oChildren = oContext.LoadDelegate.EndInvoke(oResult);
			}
			catch (Exception ex)
			{
				AddChildSilently(new ErrorModelNode(m_oModel, "Load failed (" + ex.Message + ")", ex.GetType().ToString() + ": " + ex.Message + Environment.NewLine + ex.StackTrace));
				m_eStatus = LoadState.LoadFailed;
				return;
			}

			foreach (ModelNode oChild in oChildren)
			{
				AddChildSilently(oChild);
			}

			m_eStatus = LoadState.LoadSuccessful;
		}

		internal void WaitForLoad()
		{
			if (m_oCurrentAsyncResult != null)
			{
				m_oCurrentAsyncResult.AsyncWaitHandle.WaitOne();
				while (m_eStatus == LoadState.Loading)
				{
					Thread.Sleep(0);
				}
			}
		}

		/// <summary>
		/// Gets the child ModelNodes of this ModelNode.
		/// </summary>
		[Browsable(false)]
		internal virtual ModelNode[] UnfilteredChildren
		{
			get
			{
				if (IsLeaf) return new ModelNode[0];

				lock (m_oAsyncLock)
				{
					if (m_eStatus == LoadState.Unloaded)
					{
						BeginLoad();
					}

					if (m_eStatus == LoadState.Loading)
					{
						List<ModelNode> data = new List<ModelNode>(m_oChildren);
						data.Add(new LoadingModelNode(m_oModel));
						return data.ToArray();
					}
					else
					{
						return m_oChildren.ToArray();
					}
				}
			}
		}

		[Browsable(false)]
		internal ModelNode[] FilteredChildren
		{
			get
			{
				if (!m_oModel.SearchFilterSet)
				{
					return UnfilteredChildren;
				}

				List<ModelNode> result = new List<ModelNode>();

				foreach (ModelNode oChild in UnfilteredChildren)
				{
					if (oChild is IFilterableModelNode && (oChild as IFilterableModelNode).PassesFilter || !(oChild is IFilterableModelNode) || oChild.LoadState != LoadState.LoadSuccessful)
					{
						result.Add(oChild);
					}
				}

				return result.ToArray();
			}
		}

		/// <summary>
		/// The current LoadState of this ModelNode.
		/// </summary>
		[Browsable(false)]
		internal LoadState LoadState
		{
			get { return m_eStatus; }
		}

		#endregion


		#region Public Methods

		public int CompareTo(ModelNode other)
		{
			return this.DisplayText.CompareTo(other.DisplayText);
		}

		/// <summary>
		/// Gets the zero-based index of the given child ModelNode among this ModelNode's children.
		/// </summary>
		/// <param name="oChild"></param>
		/// <returns></returns>
		internal int GetIndex(ModelNode oChild)
		{
			#region // Input checking
			if (!m_oChildren.Contains(oChild)) throw new ArgumentException("The specified node is not a child of this node.");
			#endregion

			return m_oChildren.IndexOf(oChild);
		}

		internal void ClearSilently()
		{
			m_oModel.DoWithLock(new MethodInvoker(_ClearSilentlyInModelLock));
		}

		private void _ClearSilentlyInModelLock()
		{
			m_oChildren.Clear();
		}

		internal void RemoveChild(ModelNode oChild)
		{
			if (!m_oChildren.Contains(oChild)) throw new ArgumentException("Invalid child node");

			m_oChildren.Remove(oChild);
			m_oModel.ModelNodeRemoved(this, oChild);
		}

		#endregion


		#region Helper Methods

		/// <summary>
		/// Add a ModelNode as child of this ModelNode.
		/// </summary>
		/// <param name="child">The child ModelNode to add.</param>
		protected void AddChild(ModelNode child)
		{
			AddChildSilently(child);
			m_oModel.ModelNodeAdded(this, child);
		}

		/// <summary>
		/// Add a ModelNode as a child of this ModelNode, but don't notify the Model.
		/// </summary>
		/// <remarks>
		/// Used in the async loading code, since the NodeLoaded event will cover all the
		/// nodes added in one event.
		/// </remarks>
		/// <param name="child">The child ModelNode to add.</param>
		protected void AddChildSilently(ModelNode child)
		{
			child.m_oParent = this;
			m_oChildren.Add(child);
		}

		/// <summary>
		/// Call in a constructor to denote that this node is already loaded.
		/// </summary>
		protected void MarkLoaded()
		{
			m_eStatus = LoadState.LoadSuccessful;
		}

		#endregion
	}


	/// <summary>
	/// A ModelNode representing a layer that can appear on the globe.
	/// </summary>
	internal abstract class LayerModelNode : ModelNode, IContextModelNode
	{
		#region Constructors

		internal LayerModelNode(DappleModel oModel)
			: base(oModel)
		{
		}

		#endregion


		#region Event Handlers

		protected void c_miAddLayer_Click(object sender, EventArgs e)
		{
			AddToVisibleLayers();
			OnDisplayInfoChanged();
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public virtual ToolStripMenuItem[] MenuItems
		{
			get
			{
				return new ToolStripMenuItem[] {
					new ToolStripMenuItem("Add to Data Layers", IconKeys.ImageList.Images[IconKeys.AddLayerMenuItem], new EventHandler(c_miAddLayer_Click))
				};
			}
		}

		internal bool Visible
		{
			get
			{
				return m_oModel.ViewedDatasets.Contains(this);
			}
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal abstract Dapple.LayerGeneration.LayerBuilder ConvertToLayerBuilder();

		internal void AddToVisibleLayers()
		{
			m_oModel.ViewedDatasets.Add(this);
		}

		/// <summary>
		/// Takes a LayerBuilder, adds its server to the server tree and home view, and returns it.
		/// </summary>
		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal static ServerModelNode AddServerToHomeView(DappleModel oModel, LayerBuilder oLayer)
		{
			const bool Enabled = true;
			const bool DontAddToHomeViewYet = false;
			const bool DontSubmitToDappleSearch = false;

			ServerModelNode result = null;

			// --- Add the server to the model ---

			if (oLayer is ArcIMSQuadLayerBuilder)
			{
				ArcIMSQuadLayerBuilder castLayer = oLayer as ArcIMSQuadLayerBuilder;
				result = oModel.AddArcIMSServer(new ArcIMSServerUri(castLayer.ServerURL), Enabled, DontAddToHomeViewYet, DontSubmitToDappleSearch);
			}
			else if (oLayer is DAPQuadLayerBuilder)
			{
				DAPQuadLayerBuilder castLayer = oLayer as DAPQuadLayerBuilder;
				result = oModel.AddDAPServer(new DapServerUri(castLayer.ServerURL), Enabled, DontAddToHomeViewYet, DontSubmitToDappleSearch);
			}
			else if (oLayer is WMSQuadLayerBuilder)
			{
				WMSQuadLayerBuilder castLayer = oLayer as WMSQuadLayerBuilder;
				result = oModel.AddWMSServer(new WMSServerUri(castLayer.ServerURL), Enabled, DontAddToHomeViewYet, DontSubmitToDappleSearch);
			}
			else
			{
				throw new ApplicationException("Don't know how to get the server of type " + oLayer.GetType().ToString());
			}

			result.AddToHomeView();

			return result;
		}

		#endregion
	}


	/// <summary>
	/// A ModelNode representing a server.
	/// </summary>
	internal abstract class ServerModelNode : ModelNode, IContextModelNode
	{
		#region Enums

		internal enum ServerType
		{
			DAP,
			WMS,
			ArcIMS
		}

		#endregion


		#region Member Variables

		private bool m_blEnabled;
		private bool m_blFavourite;

		protected ToolStripMenuItem m_oEnable;
		protected ToolStripMenuItem m_oSetFavourite;
		protected ToolStripMenuItem m_oRefresh;
		protected ToolStripMenuItem m_oDisable;
		protected ToolStripMenuItem m_oRemove;
		protected ToolStripMenuItem m_oProperties;

		#endregion


		#region Constructors

		internal ServerModelNode(DappleModel oModel, bool blEnabled)
			: base(oModel)
		{
			m_blEnabled = blEnabled;
			m_blFavourite = false;

			m_oEnable = new ToolStripMenuItem("Enable", IconKeys.ImageList.Images[IconKeys.EnableServerMenuItem], new EventHandler(c_miToggle_Click));
			m_oSetFavourite = new ToolStripMenuItem("Set as Favorite", IconKeys.ImageList.Images[IconKeys.MakeServerFavouriteMenuItem], new EventHandler(c_miSetFavourite_Click));
			m_oRefresh = new ToolStripMenuItem("Refresh", IconKeys.ImageList.Images[IconKeys.RefreshServerMenuItem], new EventHandler(c_miRefresh_Click));
			m_oDisable = new ToolStripMenuItem("Disable", IconKeys.ImageList.Images[IconKeys.DisableServerMenuItem], new EventHandler(c_miToggle_Click));
			m_oRemove = new ToolStripMenuItem("Remove", IconKeys.ImageList.Images[IconKeys.RemoveServerMenuItem], new EventHandler(c_miRemove_Click));
			m_oProperties = new ToolStripMenuItem("Properties", IconKeys.ImageList.Images[IconKeys.ViewServerPropertiesMenuItem], new EventHandler(c_miProperties_Click));
		}

		#endregion


		#region Event Handlers

		protected void c_miSetFavourite_Click(object sender, EventArgs e)
		{
			m_oModel.SetFavouriteServer(this, true);
		}

		protected void c_miRefresh_Click(object sender, EventArgs e)
		{
			RefreshServer();
		}

		protected void c_miToggle_Click(object sender, EventArgs e)
		{
			m_oModel.ToggleServer(this, true);
		}

		protected void c_miRemove_Click(object sender, EventArgs e)
		{
			m_oModel.RemoveServer(this, true);
		}

		protected void c_miProperties_Click(object sender, EventArgs e)
		{
			ViewProperties();
		}

		#endregion


		#region Properties

		[Browsable(false)]
		internal override ModelNode[] UnfilteredChildren
		{
			get
			{
				if (!m_blEnabled)
				{
					return new ModelNode[] 
					{
						new InformationModelNode(m_oModel, "Enable this server to view its contents")
					};
				}

				return base.UnfilteredChildren;
			}
		}

		[Browsable(false)]
		internal override string IconKey
		{
			get
			{
				if (!Enabled)
				{
					return IconKeys.DisabledServer;
				}
				else if (LoadState == LoadState.LoadFailed)
				{
					return IconKeys.OfflineServer;
				}
				else
				{
					return IconKeys.OnlineServer;
				}
			}
		}

		[Browsable(false)]
		internal abstract string ServerTypeIconKey { get; }

		[Browsable(false)]
		public virtual ToolStripMenuItem[] MenuItems
		{
			get
			{
				m_oSetFavourite.Enabled = m_blEnabled && !m_blFavourite;
				m_oRefresh.Enabled = m_blEnabled;
				m_oProperties.Enabled = m_blEnabled;

				return new ToolStripMenuItem[] {
					m_blEnabled ? m_oDisable : m_oEnable,
					m_oProperties,
					m_oRefresh,
					m_oSetFavourite,
					m_oRemove
				};
			}
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("Whether this server is currently enabled.")]
		public bool Enabled
		{
			get { return m_blEnabled; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("Whether this server is your current favourite server.")]
		public bool Favourite
		{
			get { return m_blFavourite; }
		}

		[Browsable(true)]
		[Category("Server")]
		[Description("The URI for this server.")]
		public abstract ServerUri Uri { get; }

		[Browsable(true)]
		[Category("Server")]
		[Description("What type of server (DAP, WMS, ArcIMS) this server is.")]
		public abstract ServerType Type { get; }

		#endregion


		#region Public Methods

		internal void ViewProperties()
		{
			Dapple.frmProperties.DisplayForm(this);
		}

		internal bool UpdateFavouriteStatus(String strUri)
		{
			m_blFavourite = Uri.ToString().Equals(strUri);
			return m_blFavourite;
		}

		internal void ToggleEnabled()
		{
			if (m_blEnabled == false)
			{
				m_blEnabled = true;
				BeginLoad();
			}
			else
			{
				m_blEnabled = false;
				UnloadSilently();
			}
		}

		internal void RefreshServer()
		{
			Unload();
		}

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		internal List<LayerBuilder> GetBuilders()
		{
			if (m_blEnabled)
			{
				return GetBuildersInternal();
			}
			else
			{
				return new List<LayerBuilder>();
			}
		}

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		abstract internal List<LayerBuilder> GetBuildersInternal();


		internal abstract void AddToHomeView();

		#endregion
	}


	/// <summary>
	/// Interface implemented by modelNodes that the user shouldn't be able to select in the ServerTree.
	/// </summary>
	internal interface IAnnotationModelNode
	{
	}


	/// <summary>
	/// Interface implemented by ModelNodes that have popup menus.
	/// </summary>
	internal interface IContextModelNode
	{
		[Browsable(false)]
		ToolStripMenuItem[] MenuItems { get; }
	}


	/// <summary>
	/// Interface implemented by filterable ModelNodes: those ModelNodes that have
	/// some/all of their children removed based on the current search filter.
	/// </summary>
	internal interface IFilterableModelNode
	{
		int FilteredChildCount { get; }
		bool PassesFilter { get; }
	}
}
