using System;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using WorldWind;
using System.Xml;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Utility;
using WorldWind.Net;

namespace WorldWind.Renderable
{
    /// <summary>
    /// This class Loads and Renders at a specific lat,lon,alt a given
    /// Model(ie Textured Mesh) in Direct X or Other supported Format
    /// </summary>
    internal class ModelFeature : WorldWind.Renderable.RenderableObject
    {
        #region internal Variables
        #endregion

        #region Private Variables
        private float m_latitude;
        private float m_longitude;
        private float m_altitude;
        private float m_scale = 1;
        private float m_rotx = 0, m_roty = 0, m_rotz = 0;
        private AltitudeMode m_altitudeMode = AltitudeMode.RelativeToGround;
        private bool m_isVertExaggerable = true;
        private bool m_isElevationRelativeToGround = true;
        private float storedAltitude; // to hold the old altitude value when AltitudeMode is switched to ClampedToGround
        #endregion

        #region Protected variables
        protected float currentElevation = 0;
        protected float vertExaggeration = 1;
        protected Point3d worldXyz = Point3d.Empty; // XYZ World coordinates
        protected string meshFileName;
        protected MeshElem[] meshElems;
        protected string errorMsg;
        private string m_refreshurl = null;
        private System.Timers.Timer refreshTimer;    //For mesh location polling
        #endregion

        protected class MeshElem
        {
            internal Mesh mesh;
            internal Texture[] meshTextures;            // Textures for the mesh
            internal Material[] meshMaterials;
        }

        #region Accessor Methods
        internal string RefreshURL
        {
            set { m_refreshurl = value; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets model latitude
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Latitude of model")]
        internal float Latitude
        {
            get { return m_latitude; }
            set { m_latitude = value; }
        }

        /// <summary>
        /// Gets/sets model longitude
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Longitude of model")]
        internal float Longitude
        {
            get { return m_longitude; }
            set { m_longitude = value; }
        }

        /// <summary>
        /// Gets/sets model altitude
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Altitude of model")]
        internal float Altitude
        {
            get { return m_altitude; }
            set
            {
                m_altitude = value;
                storedAltitude = m_altitude;
            }
        }

        /// <summary>
        /// Gets/sets model scale
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Scale of model")]
        internal float Scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        /// <summary>
        /// Gets/sets X rotation
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Rotation about model X-axis")]
        internal float RotX
        {
            get { return m_rotx; }
            set { m_rotx = value; }
        }

        /// <summary>
        /// Gets/sets Y rotation
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Rotation about model Y-axis")]
        internal float RotY
        {
            get { return m_roty; }
            set { m_roty = value; }
        }

        /// <summary>
        /// Gets/sets Z rotation
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Rotation about model Z-axis")]
        internal float RotZ
        {
            get { return m_rotz; }
            set { m_rotz = value; }
        }

        /// <summary>
        /// Gets/sets whether altitude obeys vert. exaggeration value from settings
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Whether model altitude scales with vertical exaggeration")]
        internal bool IsVertExaggerable
        {
            get { return m_isVertExaggerable; }
            set { m_isVertExaggerable = value; }
        }

        /// <summary>
        /// Gets/sets whether model altitude is relative to ground
        /// </summary>
        [CategoryAttribute("Model"), DescriptionAttribute("Whether model altitude is relative to ground")]
        internal bool IsElevationRelativeToGround
        {
            get { return m_isElevationRelativeToGround; }
            set { m_isElevationRelativeToGround = value; }
        }

        /// <summary>
        /// Gets/sets altitude mode
        /// </summary>
        [CategoryAttribute("Model")]
        internal AltitudeMode AltitudeMode
        {
            get { return m_altitudeMode; }
            set
            {
                m_altitudeMode = value;

                if (m_altitudeMode == AltitudeMode.Absolute)
                {
                    m_isElevationRelativeToGround = false;
                    m_altitude = storedAltitude;
                }
                if (m_altitudeMode == AltitudeMode.RelativeToGround)
                {
                    m_isElevationRelativeToGround = true;
                    m_altitude = storedAltitude;
                }
                if (m_altitudeMode == AltitudeMode.ClampedToGround)
                    m_altitude = 0;

            }
        }
        #endregion


        internal ModelFeature(string name, World parentWorld, string fileName, float Latitude,
            float Longitude, float Altitude, float Scale, float rotX, float rotY, float rotZ)
            : base(name, parentWorld)
        {
            meshFileName = fileName;
            this.m_latitude = Latitude;
            this.m_longitude = Longitude;
            this.m_altitude = Altitude;
            this.m_scale = Scale;
            this.m_rotx = rotX;
            this.m_roty = rotY;
            this.m_rotz = rotZ;
            this.storedAltitude = Altitude;
            this.meshElems = null;
        }

        /// <summary>
        /// Determine if the object is visible
        /// </summary>
        protected bool IsVisible(WorldWind.Camera.CameraBase camera)
        {
            if (worldXyz == Point3d.Empty)
                worldXyz = MathEngine.SphericalToCartesian(Latitude, Longitude, camera.WorldRadius);
            return camera.ViewFrustum.ContainsPoint(worldXyz);
        }

		  public override void Render(DrawArgs drawArgs)
        {
            if (errorMsg != null)
            {
                errorMsg = null;
                IsOn = false;
                isInitialized = false;
                return;
            }

            if (!IsVisible(drawArgs.WorldCamera))
            {
                // Mesh is not in view, unload it to save memory
                if (isInitialized)
                    Dispose();
                return;
            }

            if (!isInitialized)
                return;

            drawArgs.device.RenderState.CullMode = Cull.None;
            drawArgs.device.RenderState.Lighting = true;
            drawArgs.device.RenderState.AmbientColor = 0x808080;
            drawArgs.device.RenderState.NormalizeNormals = true;

            drawArgs.device.Lights[0].Diffuse = Color.FromArgb(255, 255, 255);
            drawArgs.device.Lights[0].Type = LightType.Directional;
            drawArgs.device.Lights[0].Direction = new Vector3(1f, 1f, 1f);
            drawArgs.device.Lights[0].Enabled = true;

            drawArgs.device.SamplerState[0].AddressU = TextureAddress.Wrap;
            drawArgs.device.SamplerState[0].AddressV = TextureAddress.Wrap;

            drawArgs.device.RenderState.AlphaBlendEnable = true;
            drawArgs.device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
            drawArgs.device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;

            // Put the light somewhere up in space
            drawArgs.device.Lights[0].Position = new Vector3(
                (float)worldXyz.X * 2f,
                (float)worldXyz.Y * 1f,
                (float)worldXyz.Z * 1.5f);

            Matrix currentWorld = drawArgs.device.Transform.World;
            drawArgs.device.Transform.World = Matrix.RotationX((float)MathEngine.DegreesToRadians(RotX));
            drawArgs.device.Transform.World *= Matrix.RotationY((float)MathEngine.DegreesToRadians(RotY));
            drawArgs.device.Transform.World *= Matrix.RotationZ((float)MathEngine.DegreesToRadians(RotZ));
            drawArgs.device.Transform.World *= Matrix.Scaling(Scale, Scale, Scale);

            // Move the mesh to desired location on earth
            if (IsVertExaggerable == true)
                vertExaggeration = World.Settings.VerticalExaggeration;
            else vertExaggeration = 1;
            drawArgs.device.Transform.World *= Matrix.Translation(0, 0, (float)drawArgs.WorldCamera.WorldRadius + (currentElevation * Convert.ToInt16(m_isElevationRelativeToGround) + Altitude) * vertExaggeration);
            drawArgs.device.Transform.World *= Matrix.RotationY((float)MathEngine.DegreesToRadians(90 - Latitude));
            drawArgs.device.Transform.World *= Matrix.RotationZ((float)MathEngine.DegreesToRadians(Longitude));


            drawArgs.device.Transform.World *= Matrix.Translation(
                (float)-drawArgs.WorldCamera.ReferenceCenter.X,
                (float)-drawArgs.WorldCamera.ReferenceCenter.Y,
                (float)-drawArgs.WorldCamera.ReferenceCenter.Z
                );


            foreach (MeshElem me in meshElems)
            {
                for (int i = 0; i < me.meshMaterials.Length; i++)
                {
                    // Set the material and texture for this subset
                    drawArgs.device.Material = me.meshMaterials[i];
                    drawArgs.device.SetTexture(0, me.meshTextures[i]);

                    // Draw the mesh subset
                    me.mesh.DrawSubset(i);
                }
            }

            drawArgs.device.Transform.World = currentWorld;
            drawArgs.device.RenderState.Lighting = false;
        }


        /// <summary>
        /// RenderableObject abstract member (needed) 
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
		  public override void Initialize(DrawArgs drawArgs)
        {
            if (!IsVisible(drawArgs.WorldCamera))
                return;
            if (meshFileName.StartsWith("http"))
            {
                Uri meshuri = new Uri(meshFileName);
                string meshpath = meshuri.AbsolutePath;
                string extension = Path.GetExtension(meshpath);
                //download online mesh files to cache and
                //update meshfilename to new name
                if (meshuri.Scheme == Uri.UriSchemeHttp
                    || meshuri.Scheme == Uri.UriSchemeHttps)
                {
                    try
                    {
                        WebDownload request = new WebDownload(meshFileName);

                        string cachefilename = request.GetHashCode() + extension;
                        //HACK: Hard Coded Path
                        cachefilename =
                            Directory.GetParent(System.Windows.Forms.Application.ExecutablePath) +
                            "//Cache//Models//" + cachefilename;
                        if (!File.Exists(cachefilename))
                            request.DownloadFile(cachefilename);
                        meshFileName = cachefilename;
                    }
                    catch (Exception caught)
                    {
                        Utility.Log.Write(caught);
                        errorMsg = "Failed to download mesh from " + meshFileName;
                    }
                }
            }
            string ext = Path.GetExtension(meshFileName);
            try
            {
                if (ext.Equals(".x"))
                    LoadDirectXMesh(drawArgs);
                else if (ext.Equals(".dae") || ext.Equals(".xml"))
                    LoadColladaMesh(drawArgs);
                if (meshElems == null)
                    throw new InvalidMeshException();

                vertExaggeration = World.Settings.VerticalExaggeration;
                if (m_isElevationRelativeToGround == true)
                    currentElevation = World.TerrainAccessor.GetElevationAt(Latitude, Longitude);

                if (refreshTimer == null && m_refreshurl != null)
                {
                    refreshTimer = new System.Timers.Timer(60000);
                    refreshTimer.Elapsed += new System.Timers.ElapsedEventHandler(refreshTimer_Elapsed);
                    refreshTimer.Start();
                }

                isInitialized = true;
            }
            catch (Exception caught)
            {
                Utility.Log.Write(caught);
                errorMsg = "Failed to read mesh from " + meshFileName;
            }
        }

        /// <summary>
        /// Method to load Native Direct X Meshes
        /// </summary>
        private void LoadDirectXMesh(DrawArgs drawArgs)
        {
            meshElems = new MeshElem[1];

            ExtendedMaterial[] materials;
            GraphicsStream adj;
            meshElems[0] = new MeshElem();
            meshElems[0].mesh = Mesh.FromFile(meshFileName, MeshFlags.Managed, drawArgs.device, out adj, out materials);

            // Extract the material properties and texture names.
            meshElems[0].meshTextures = new Texture[materials.Length];
            meshElems[0].meshMaterials = new Material[materials.Length];
            string xFilePath = Path.GetDirectoryName(meshFileName);
            for (int i = 0; i < materials.Length; i++)
            {
                meshElems[0].meshMaterials[i] = materials[i].Material3D;
                // Set the ambient color for the material (D3DX does not do this)
                meshElems[0].meshMaterials[i].Ambient = meshElems[0].meshMaterials[i].Diffuse;

                // Create the texture.
                if (materials[i].TextureFilename != null)
                {
                    string textureFilePath = Path.Combine(xFilePath, materials[i].TextureFilename);
                    meshElems[0].meshTextures[i] = TextureLoader.FromFile(drawArgs.device, textureFilePath);
                }
            }
        }

        private void LoadColladaMesh(DrawArgs drawArgs)
        {
            XmlDocument colladaDoc = new XmlDocument();
            colladaDoc.Load(meshFileName);
            XmlNamespaceManager xmlnsManager = new XmlNamespaceManager(colladaDoc.NameTable);
            xmlnsManager.AddNamespace("c", colladaDoc.GetElementsByTagName("COLLADA")[0].NamespaceURI);

            int meshCount =
                    colladaDoc.SelectNodes("//c:COLLADA/c:library_geometries/c:geometry/c:mesh",
                    xmlnsManager).Count;
            if (meshCount == 0)
                return;
            if (meshElems != null)
            {
                foreach (MeshElem me in meshElems)
                {
                    if (me.mesh != null)
                        me.mesh.Dispose();
                    if (me.meshTextures != null)
                        foreach (Texture t in me.meshTextures)
                            if (t != null)
                                t.Dispose();
                }
            }
            meshElems = new MeshElem[meshCount];

            for (int j = 0; j < meshCount; j++)
            {
                XmlNode meshNode = colladaDoc.SelectNodes(
                        "//c:COLLADA/c:library_geometries/c:geometry/c:mesh",
                        xmlnsManager)[j];

                Matrix transform = Matrix.Identity;
                string sGeometryId = meshNode.SelectNodes("../@id", xmlnsManager)[0].Value;
                if (colladaDoc.SelectNodes("//c:COLLADA/c:library_visual_scenes/c:visual_scene/c:node" +
                        "[c:instance_geometry/@url='#" + sGeometryId + "']", xmlnsManager).Count == 1)
                {
                    XmlNode sceneNode =
                            colladaDoc.SelectNodes("//c:COLLADA/c:library_visual_scenes/c:visual_scene/c:node" +
                            "[c:instance_geometry/@url='#" + sGeometryId + "']", xmlnsManager)[0];

                    foreach (XmlNode childNode in sceneNode.ChildNodes)
                    {
                        Matrix m;
                        if (childNode.Name == "translate")
                        {
                            string[] translateParams = childNode.InnerText.Trim().Split(' ');
                            m = Matrix.Translation(new Vector3(
                                    System.Decimal.ToSingle(System.Decimal.Parse(
                                        translateParams[0],
                            System.Globalization.NumberStyles.Any)),
                                    System.Decimal.ToSingle(System.Decimal.Parse(
                                        translateParams[1],
                            System.Globalization.NumberStyles.Any)),
                                    System.Decimal.ToSingle(System.Decimal.Parse(
                                        translateParams[2],
                                        System.Globalization.NumberStyles.Any))));

                        }
                        else if (childNode.Name == "rotate")
                        {
                            string[] rotateParams = childNode.InnerText.Trim().Split(' ');
                            m = Matrix.RotationAxis(
                                    new Vector3(
                                        System.Decimal.ToSingle(System.Decimal.Parse(
                                            rotateParams[0],
                            System.Globalization.NumberStyles.Any)),
                                        System.Decimal.ToSingle(System.Decimal.Parse(
                                            rotateParams[1],
                            System.Globalization.NumberStyles.Any)),
                                        System.Decimal.ToSingle(System.Decimal.Parse(
                                            rotateParams[2],
                                            System.Globalization.NumberStyles.Any))),
                                    (float)Math.PI * System.Decimal.ToSingle(System.Decimal.Parse(
                                        rotateParams[3],
                                        System.Globalization.NumberStyles.Any)) / 180.0f);


                        }
                        else if (childNode.Name == "scale")
                        {
                            string[] scaleParams = childNode.InnerText.Trim().Split(' ');
                            m = Matrix.Scaling(new Vector3(
                                    System.Decimal.ToSingle(System.Decimal.Parse(
                                        scaleParams[0],
                                        System.Globalization.NumberStyles.Any)),
                                    System.Decimal.ToSingle(System.Decimal.Parse(
                                        scaleParams[1],
                                        System.Globalization.NumberStyles.Any)),
                                    System.Decimal.ToSingle(System.Decimal.Parse(
                                        scaleParams[2],
                                        System.Globalization.NumberStyles.Any))));
                        }
                        else
                        {
                            continue;
                        }
                        transform = Matrix.Multiply(m, transform);
                    }

                }



                string sVertSource = meshNode.SelectNodes(
                        "c:vertices/c:input[@semantic='POSITION']/@source",
                        xmlnsManager)[0].Value;
                int iVertCount = System.Decimal.ToInt32(System.Decimal.Parse(
                        meshNode.SelectNodes(
                                "c:source[@id='" +
                                sVertSource.Substring(1) +
                                "']/c:float_array/@count", xmlnsManager)[0].Value)) / 3;
                string[] vertCoords = meshNode.SelectNodes(
                        "c:source[@id='" +
                        sVertSource.Substring(1) +
                        "']/c:float_array", xmlnsManager)[0].InnerText.Trim().Split(' ');
                CustomVertex.PositionNormalTextured[] vertices =
                        new CustomVertex.PositionNormalTextured[iVertCount];
                Vector3 v = new Vector3();
                for (int i = 0; i < iVertCount; i++)
                {
                    v.X = System.Decimal.ToSingle(System.Decimal.Parse(vertCoords[i * 3 + 0],
                                System.Globalization.NumberStyles.Any));
                    v.Y = System.Decimal.ToSingle(System.Decimal.Parse(vertCoords[i * 3 + 1],
                                System.Globalization.NumberStyles.Any));
                    v.Z = System.Decimal.ToSingle(System.Decimal.Parse(vertCoords[i * 3 + 2],
                                System.Globalization.NumberStyles.Any));
                    v.TransformCoordinate(transform);

                    vertices[i] = new CustomVertex.PositionNormalTextured(
                            v.X, v.Y, v.Z, 0.0f, 0.0f, 0.0f, v.X, v.Y);
                }
                int iFaceCount = System.Decimal.ToInt32(System.Decimal.Parse(
                            meshNode.SelectNodes(
                                    "c:triangles/@count",
                                xmlnsManager)[0].Value));

                string[] triVertIndicesStr = meshNode.SelectNodes(
                        "c:triangles/c:p",
                    xmlnsManager)[0].InnerText.Trim().Split(' '); ;
                short[] triVertIndices = new short[triVertIndicesStr.Length];
                for (int i = 0; i < triVertIndicesStr.Length; i++)
                {
                    triVertIndices[i] = System.Decimal.ToInt16(System.Decimal.Parse(triVertIndicesStr[i]));
                }

                meshElems[j] = new MeshElem();
                meshElems[j].mesh = new Mesh(iFaceCount, iVertCount, 0, CustomVertex.PositionNormalTextured.Format, drawArgs.device);
                meshElems[j].mesh.SetVertexBufferData(vertices, LockFlags.None);
                meshElems[j].mesh.SetIndexBufferData(triVertIndices, LockFlags.None);

                int[] adjacency = new int[meshElems[j].mesh.NumberFaces * 3];
                meshElems[j].mesh.GenerateAdjacency(0.000000001F, adjacency);
                meshElems[j].mesh.OptimizeInPlace(MeshFlags.OptimizeVertexCache, adjacency);
                meshElems[j].mesh.ComputeNormals();

                int numSubSets = meshElems[j].mesh.GetAttributeTable().Length;
                meshElems[j].meshTextures = new Texture[numSubSets];
                meshElems[j].meshMaterials = new Material[numSubSets];

                for (int i = 0; i < numSubSets; i++)
                {
                    meshElems[j].meshMaterials[i].Ambient = Color.FromArgb(255, 255, 43, 48);
                    meshElems[j].meshMaterials[i].Diffuse = Color.FromArgb(255, 155, 113, 148);
                    meshElems[j].meshMaterials[i].Emissive = Color.FromArgb(255, 255, 143, 98);
                    meshElems[j].meshMaterials[i].Specular = Color.FromArgb(255, 155, 243, 48);
                }
            }

        }

        /// <summary>
        /// RenderableObject abstract member (needed)
        /// OBS: Worker thread (d
        /// on't update UI directly from this thread)
        /// </summary>
		  public override void Update(DrawArgs drawArgs)
        {
            if (!isInitialized)
                Initialize(drawArgs);
            else if ((vertExaggeration != World.Settings.VerticalExaggeration) && (IsVertExaggerable == true))
                Initialize(drawArgs);
            else if ((m_isElevationRelativeToGround == true) && (currentElevation != World.TerrainAccessor.GetElevationAt(Latitude, Longitude)))
                Initialize(drawArgs);

        }

        bool isUpdating = false;
        private void refreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (isUpdating)
                return;
            //refresh object position
            if (m_refreshurl != null)
            {
                WebDownload dl = new WebDownload(m_refreshurl);
                dl.SavedFilePath = meshFileName.Replace(".x", ".txt");
                dl.CompleteCallback += new DownloadCompleteHandler(positionupdateComplete);
                dl.BackgroundDownloadFile();
            }
            isUpdating = true;
        }

        private void positionupdateComplete(WebDownload wd)
        {
            //TODO: Update Object Position
            try
            {
                wd.Verify();
                StreamReader reader = File.OpenText(wd.SavedFilePath);
                string[] location = reader.ReadLine().Split(new char[] { ',' });
                this.Latitude = Convert.ToSingle(location[0]);
                this.Longitude = Convert.ToSingle(location[1]);
                this.Altitude = Convert.ToSingle(location[2]);
            }
            finally
            {
                isUpdating = false;
            }
        }

        /// <summary>
        /// RenderableObject abstract member (needed)
        /// OBS: Worker thread (don't update UI directly from this thread)
        /// </summary>
		  public override void Dispose()
        {
            isInitialized = false;
            if (meshElems != null)
            {
                foreach (MeshElem me in meshElems)
                {
                    if (me.mesh != null)
                        me.mesh.Dispose();
                    if (me.meshTextures != null)
                        foreach (Texture t in me.meshTextures)
                            if (t != null)
                                t.Dispose();
                }
            }

        }

        /// <summary>
        /// Gets called when user left clicks.
        /// RenderableObject abstract member (needed)
        /// Called from UI thread = UI code safe in this function
        /// </summary>
		  public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            // Note: The following assignment to m is
            // to match the assignment to 
            // drawArgs.device.Transform.World in Render().
            // Changes to that matrix should be mirrored here
            // until the assignment is centralized.

            Matrix4d m = Matrix4d.RotationX(MathEngine.DegreesToRadians(RotX));
            m *= Matrix4d.RotationY(MathEngine.DegreesToRadians(RotY));
            m *= Matrix4d.RotationZ(MathEngine.DegreesToRadians(RotZ));
            m *= Matrix4d.Scaling(Scale, Scale, Scale);

            if (IsVertExaggerable == true)
                vertExaggeration = World.Settings.VerticalExaggeration;
            else vertExaggeration = 1;
            m *= Matrix4d.Translation(0, 0, drawArgs.WorldCamera.WorldRadius + (currentElevation * Convert.ToInt16(m_isElevationRelativeToGround) + Altitude) * vertExaggeration);
            m *= Matrix4d.RotationY(MathEngine.DegreesToRadians(90 - Latitude));
            m *= Matrix4d.RotationZ(MathEngine.DegreesToRadians(Longitude));

            m *= Matrix4d.Translation(
                -drawArgs.WorldCamera.ReferenceCenter.X,
                -drawArgs.WorldCamera.ReferenceCenter.Y,
                -drawArgs.WorldCamera.ReferenceCenter.Z
                );

            m = Matrix4d.Invert(m);

            Point3d v1 = new Point3d();
            v1.X = DrawArgs.LastMousePosition.X;
            v1.Y = DrawArgs.LastMousePosition.Y;
            v1.Z = drawArgs.WorldCamera.Viewport.MinZ;
            v1.Unproject(drawArgs.WorldCamera.Viewport,
                    drawArgs.WorldCamera.ProjectionMatrix,
                    drawArgs.WorldCamera.ViewMatrix,
                    drawArgs.WorldCamera.WorldMatrix);

            Point3d v2 = new Point3d();
            v2.X = DrawArgs.LastMousePosition.X;
            v2.Y = DrawArgs.LastMousePosition.Y;
            v2.Z = drawArgs.WorldCamera.Viewport.MaxZ;
            v2.Unproject(drawArgs.WorldCamera.Viewport,
                    drawArgs.WorldCamera.ProjectionMatrix,
                    drawArgs.WorldCamera.ViewMatrix,
                    drawArgs.WorldCamera.WorldMatrix);

            v1.TransformCoordinate(m);
            v2.TransformCoordinate(m);

            bool sel = false;
            foreach (MeshElem me in meshElems)
            {
                sel |= (me.mesh.Intersect(v1.Vector3, (v2 - v1).Vector3));
            }
            if (sel)
            {
            }

            return sel;
        }

    }
}
