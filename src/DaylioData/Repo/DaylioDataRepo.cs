﻿using DaylioData.Models;

namespace DaylioData.Repo
{
    /// <summary>
    /// <see cref="DaylioDataRepo"/> Repository for Daylio data read from a CSV file.
    /// </summary>
    public class DaylioDataRepo
    {

        private IEnumerable<DaylioCSVDataModel>? _CSVData;

        public IEnumerable<DaylioCSVDataModel>? CSVData => _CSVData;
        public HashSet<string> Activities = new HashSet<string>();

        internal DaylioDataRepo(DaylioFileAccess fileAccess)
        {

            _CSVData = fileAccess.TryReadFile();
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

    }
}
