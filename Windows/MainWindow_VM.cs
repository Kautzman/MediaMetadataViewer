using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using FFMediaToolkit;
using FFMediaToolkit.Decoding;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;
using System.Reflection;

namespace MediaMetadataViewer
{
    class MainWindow_VM : BindableBase
    {
        string[] extensions = { ".mp4", ".mov", ".mkv" };

        private string _rootDir = "";
        public string RootDir
        {
            get => _rootDir;
            set => SetProperty(ref _rootDir, value);
        }

        private bool _getSubDirectories = true;
        public bool GetSubDirectories
        {
            get => _getSubDirectories;
            set => SetProperty(ref _getSubDirectories, value);
        }

        private ObservableCollection<MovieData> _movies = new ObservableCollection<MovieData>();
        public ObservableCollection<MovieData> Movies
        {
            get => _movies;
            set => SetProperty(ref _movies, value);
        }

        private ObservableCollection<MovieData> _filteredMovies = new ObservableCollection<MovieData>();
        public ObservableCollection<MovieData> FilteredMovies
        {
            get => new ObservableCollection<MovieData>(Movies.Where(i => MovieFilter(i)));
            set => SetProperty(ref _filteredMovies, value);
        }

        private String _searchText = "";
        public String SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                RaisePropertyChanged("FilteredMovies");
            }
        }

        private int _filesCount = 0;
        public int FilesCount
        {
            get => _filesCount;
            set => SetProperty(ref _filesCount, value);
        }

        private int _filesScanned = 0;
        public int FilesScanned
        {
            get => _filesScanned;
            set => SetProperty(ref _filesScanned, value);
        }

        private string _scanTime = "";
        public string ScanTime
        {
            get => _scanTime;
            set => SetProperty(ref _scanTime, value);
        }

        public DelegateCommand SetDirectoryCommand { get; private set; }
        public DelegateCommand ScanDirectoryCommand { get; private set; }
        public DelegateCommand ExportCommand { get; private set; }


        public MainWindow_VM()
        {
            FFmpegLoader.FFmpegPath = @$"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}/dlls/";
            RegisterCommands();
        }

        public void RegisterCommands()
        {
            SetDirectoryCommand = new DelegateCommand(SetDirectory);
            ScanDirectoryCommand = new DelegateCommand(async () => await ScanDirectory());
            ExportCommand = new DelegateCommand(Export);
        }

        private void SetDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    RootDir = dialog.SelectedPath;
                }
            }
        }

        private async Task ScanDirectory()
        {
            Task task = Task.Run(() => ScanDirectoryAsync());
        }

        private void ScanDirectoryAsync()
        {
            Stopwatch clock = new();
            clock.Start();

            DirectoryInfo dirInfo = new DirectoryInfo(SpecialDirectories.MyDocuments);

            try
            {
                dirInfo = new DirectoryInfo(RootDir);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("You didn't pick a directory probably, or it's not valid or something");
                return;
            }

            List<FileInfo> files = dirInfo.GetFiles("*", GetSubDirectories ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly)
                .Where(file => extensions.Any(pattern => file.Name.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))).ToList();

            FilesCount = files.Count;
            FilesScanned = 0;

            try
            {
                foreach (FileInfo file in files)
                {
                    MediaFile mediaFile = MediaFile.Open(file.FullName);

                    VideoStreamInfo info = mediaFile.Video.Info;
                    string frameRateInfo = info.IsVariableFrameRate ? "average" : "constant";

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        MovieData nextMovie = new MovieData(file.DirectoryName, file.Name.Replace(',', ' '), info.FrameSize.Width, info.FrameSize.Height, $"{Math.Round(info.AvgFrameRate, 2)} fps ({frameRateInfo})", mediaFile.Info.Bitrate / 1000.0);
                        Movies.Add(nextMovie);
                    });

                    FilesScanned++;
                }

                RaisePropertyChanged("FilteredMovies");
            }
            catch (Exception ex)
            {
                clock.Stop();
                System.Windows.MessageBox.Show("Uh, this is probably a permissions issue. Run as Admin and try again...\n\n" + ex.Message);
                ScanTime = "N/A";
                return;
            }

            clock.Stop();
            ScanTime = Math.Round(clock.ElapsedMilliseconds / 1000.0, 1).ToString();
        }

        private void Export()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.FileName = "output.csv";
            saveFileDialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";
            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                string output = saveFileDialog.FileName;

                StreamWriter csv = new StreamWriter(output);

                csv.WriteLine("Directory, Name, Width, Height, Frame Rate, Bitrate");

                foreach (MovieData movie in Movies)
                {
                    csv.WriteLine($"{movie.Directory}, {movie.Name}, {movie.Width}, {movie.Height}, {movie.FrameRate}, {movie.Bitrate}");
                }

                csv.Close();
            }
        }

        private bool MovieFilter(MovieData item)
        {
            return item.Name.ToUpper().Contains(SearchText.ToUpper()) || item.Directory.ToUpper().Contains(SearchText.ToUpper()) || SearchText.Length == 0;
        }
    }
}
