﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace XRouter.Common
{
    /// <summary>
    /// Provides read access to previously logged trace log entries.
    /// </summary>
    public class TraceLogReader : AbstractLogReader<TraceLogEntry>
    {
        public TraceLogReader(string logFilesDirectory, string serviceName)
            : base(logFilesDirectory)
        {
            LogFilePattern = string.Format("*_{0}.Trace.log", serviceName);
        }

        protected override void GetEntriesFromFile(
            string logFilePath,
            DateTime date,
            DateTime minDate,
            DateTime maxDate,
            LogLevelFilters logLevelFilter,
            ref List<TraceLogEntry> matchingEntries)
        {
            string fileContent = ReadAllText(logFilePath);
            string xmlContent = string.Format("<log>{0}</log>", fileContent);
            XDocument xLog = XDocument.Parse(xmlContent);
            foreach (XElement xEntry in xLog.Root.Elements())
            {
                TraceLogEntry entry = new TraceLogEntry(xEntry);
                if ((entry.Created >= minDate) &&
                    (entry.Created <= maxDate) &&
                    IsMatchingLogLevelFilter(entry.LogLevel, logLevelFilter))
                {
                    matchingEntries.Add(entry);
                }
            }
        }

        private string ReadAllText(string logFilePath)
        {
            List<string> lines = new List<string>();
            using (FileStream fileStream = new FileStream(logFilePath,
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = reader.ReadLine();
                }
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
