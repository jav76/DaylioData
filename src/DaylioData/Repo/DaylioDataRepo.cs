using DaylioData.Models;

namespace DaylioData.Repo
{
    /// <summary>
    /// <see cref="DaylioDataRepo"/> Repository for Daylio data read from a CSV file.
    /// </summary>
    public class DaylioDataRepo : IDisposable
    {

        private IEnumerable<DaylioCSVDataModel>? _CSVData;
        private DaylioFileAccess? _fileAccess;
        private bool disposedValue;

        internal event EventHandler<EventArgs>? DataChanged;

        public IEnumerable<DaylioCSVDataModel>? CSVData => _CSVData;
        public HashSet<string> Activities = new HashSet<string>();

        internal DaylioDataRepo(DaylioFileAccess fileAccess)
        {
            _fileAccess = fileAccess;
            _fileAccess.FileChanged += DataChangedEventHandler;
            _CSVData = _fileAccess.TryReadFile();
            InitializeActivities();
        }

        internal void DataChangedEventHandler(object? sender, EventArgs e)
        {
            _CSVData = _fileAccess?.TryReadFile();
            Activities.Clear();
            InitializeActivities();
        }

        private void InitializeActivities()
        {
            if (_CSVData == null)
            {
                return;
            }

            foreach (string activitiy in _CSVData.Select(x => x.Activities?.Split(" | ")).SelectMany(x => x ?? Array.Empty<string>()))
            {
                Activities.Add(activitiy);
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_fileAccess != null)
                    {
                        _fileAccess.FileChanged -= DataChangedEventHandler;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DaylioDataRepo()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    #endregion
}
