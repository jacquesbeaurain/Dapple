using System;
using System.Collections.Generic;
using System.Text;
using WorldWind;
using WorldWind.Renderable;

namespace WindowsApplication1
{
    class LayerBuilderProvider
    {
    }

    class DAPBuilderProvider
    {
    }

    class WMSBuilderProvider
    {
    }

    interface IBuilder
    {

        string Name
        {
            get;
        }

        byte Opacity
        {
            get;
            set;
        }

        string Type
        {
            get;
        }

        RenderableObject GetLayer();

    }

    interface ImageBuilder : IBuilder
    {
        GeographicBoundingBox Extents
        {
            get;
        }

        int ImagePixelSize
        {
            get;
            set;
        }

        object StyleTag
        {
            get;
            set;
        }
    }

    interface QuadBuilder : ImageBuilder
    {
        int LevelZeroTileSize
        {
            get;
            set;
        }

        int Levels
        {
            get;
            set;
        }

    }
}
