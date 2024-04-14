using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaMetadataViewer
{
    class MovieData
    {
        public string Directory {get; set; }
        public string Name {get; set; }
        public int Width {get; set; }
        public int Height {get; set; }  
        public string FrameRate {get; set; }
        public double  Bitrate {get; set; }

        public MovieData(string directory, string name, int width, int height, string frameRate, double bitrate)
        {
            Directory = directory;
            Name = name;
            Width = width;
            Height = height;
            FrameRate = frameRate;
            Bitrate = bitrate;
        }
    }
}
