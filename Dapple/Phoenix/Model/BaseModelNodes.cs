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
	public enum LoadState
	{
		Unloaded,
		Loading,
		LoadFailed,
		LoadSuccessful
	}


	/// <summary>
	/// The root of the ModelNode inheritance hierarchy.
	/// </summary>
	public abstract class ModelNode : IComparable<ModelNode>
	{
		#region Constants

		/// <summary>
		/// Set to true to enable some ModelNodes to always display all of their children when
		/// open, regardless of whether they are the selected node or not.
		/// </summary>
		public const bool UseShowAllChildren = false;

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

		public ModelNode(DappleModel oModel)
		{
			m_oModel = oModel;
		}

		#endregion


		#region Properties

		/// <summary>
		/// Get the parent ModelNode of this ModelNode.
		/// </summary>
		[Browsable(false)]
		public ModelNode Parent
		{
			get { return m_oParent; }
		}

		/// <summary>
		/// Whether this ModelNode is a leaf node (no children, don't collapse parent in 
		/// ServerTree when selected).
		/// </summary>
		[Browsable(false)]
		public virtual bool IsLeaf
		{
			get { return false; }
		}

		/// <summary>
		/// Whether to disallow pruning of this ModelNode's child nodes in the ServerTree.
		/// </summary>
		[Browsable(false)]
		public virtual bool ShowAllChildren
		{
			get { return false; }
		}

		/// <summary>
		/// Get a String describing this ModelNode.
		/// </summary>
		public abstract String DisplayText { get; }

		/// <summary>
		/// Get a String annotating this ModelNode (such as the number of datasets in a server).
		/// </summary>
		public abstract String Annotation { get; }

		/// <summary>
		/// The ImageKey of the TreeNode for this ModelNode.
		/// </summary>
		[Browsable(false)]
		public abstract String IconKey { get; }

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

			public AsyncContext(int iSyncNumber, LoadDelegate oDelegate)
			{
				m_iLoadSync = iSyncNumber;
				m_oDelegate = oDelegate;
			}

			public int SyncNumber { get { return m_iLoadSync; } }
			public LoadDelegate LoadDelegate { get { return m_oDelegate; } }
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

		public void UnloadSilently()
		{
			m_oModel.DoWithLock(new MethodInvoker(_UnloadInModelLock));
		}

		/// <summary>
		/// Unloads this ModelNode: its children are cleared, and its LoadState is reset to Unloaded.
		/// </summary>
		public void Unload()
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
		public void BeginLoad()
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
				AddChildSilently(new ErrorModelNode(m_oModel, "Load failed (" + ex.Message + ")"));
				m_eStatus = LoadState.LoadFailed;
				return;
			}

			foreach (ModelNode oChild in oChildren)
			{
				AddChildSilently(oChild);
			}

			m_eStatus = LoadState.LoadSuccessful;
		}

		public void WaitForLoad()
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
		public virtual ModelNode[] UnfilteredChildren
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
		public ModelNode[] FilteredChildren
		{
			get
			{
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
		public LoadState LoadState
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
		public int GetIndex(ModelNode oChild)
		{
			#region // Input checking
			if (!m_oChildren.Contains(oChild)) throw new ArgumentException("The specified node is not a child of this node.");
			#endregion

			return m_oChildren.IndexOf(oChild);
		}

		public void ClearSilently()
		{
			m_oModel.DoWithLock(new MethodInvoker(_ClearSilentlyInModelLock));
		}

		private void _ClearSilentlyInModelLock()
		{
			m_oChildren.Clear();
		}

		public void RemoveChild(ModelNode oChild)
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
	public abstract class LayerModelNode : ModelNode, IContextModelNode
	{
		#region Constructors

		public LayerModelNode(DappleModel oModel)
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

		public bool Visible
		{
			get
			{
				return m_oModel.ViewedDatasets.Contains(this);
			}
		}

		#endregion


		#region Public Methods

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		public abstract Dapple.LayerGeneration.LayerBuilder ConvertToLayerBuilder();

		public void AddToVisibleLayers()
		{
			m_oModel.ViewedDatasets.Add(this);
		}

		#endregion
	}


	/// <summary>
	/// A ModelNode representing a server.
	/// </summary>
	public abstract class ServerModelNode : ModelNode, IContextModelNode
	{
		#region Enums

		public enum ServerType
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

		public ServerModelNode(DappleModel oModel, bool blEnabled)
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
			Dapple.frmProperties.DisplayForm(this);
		}

		#endregion


		#region Properties

		[Browsable(false)]
		public override ModelNode[] UnfilteredChildren
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
		public override string IconKey
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
		public abstract string ServerTypeIconKey { get; }

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

		public bool UpdateFavouriteStatus(String strUri)
		{
			m_blFavourite = Uri.ToString().Equals(strUri);
			return m_blFavourite;
		}

		public void ToggleEnabled()
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

		public void RefreshServer()
		{
			Unload();
		}

		public void ViewServerProperties()
		{
			throw new NotImplementedException();
		}

		[Obsolete("This should get removed with the rest of the LayerBuilder/ServerTree stuff")]
		public List<LayerBuilder> GetBuilders()
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
		abstract public List<LayerBuilder> GetBuildersInternal();

		#endregion
	}


	/// <summary>
	/// Interface implemented by modelNodes that the user shouldn't be able to select in the ServerTree.
	/// </summary>
	public interface IAnnotationModelNode
	{
	}


	/// <summary>
	/// Interface implemented by ModelNodes that have popup menus.
	/// </summary>
	public interface IContextModelNode
	{
		[Browsable(false)]
		ToolStripMenuItem[] MenuItems { get; }
	}


	/// <summary>
	/// Interface implemented by filterable ModelNodes: those ModelNodes that have
	/// some/all of their children removed based on the current search filter.
	/// </summary>
	public interface IFilterableModelNode
	{
		int FilteredChildCount { get; }
		bool PassesFilter { get; }
	}
}
