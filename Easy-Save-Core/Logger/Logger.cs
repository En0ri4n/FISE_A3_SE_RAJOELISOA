using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore.Utilities
{
    public enum Format
    {
        Json,
        Xml
    }

    /// <summary>
    ///     Represents a logger that handles logging messages to both daily and status log files.
    ///     It provides methods to save logs in different formats (JSON or XML).
    ///     It also implements the singleton pattern to ensure only one instance of the logger exists.
    /// </summary>
    public class Logger
    {
        private static readonly Logger Instance = new Logger();
        private readonly object _lockObject = new object();
        /// <summary>
        ///     Singleton instance of the Logger class.
        /// </summary>
        private Logger()
        {
            DailyLogPath = @"logs\daily\";
            StatusLogPath = @"logs\status\";
            DailyLogFormat = Format.Json;
        }

        public string DailyLogPath { get; set; }

        public string StatusLogPath { get; set; }

        public Format DailyLogFormat { get; set; }

        /// <summary>
        ///     Logs a message to the status log file.
        /// </summary>
        public static void Log(LogLevel level, string message)
        {
            Instance.LogInternal(level, message);
        }

        public void LogInternal(LogLevel level, string message)
        {
            lock (_lockObject)
            {
                // _internalLogger.Log(level, message);
                LogToFile(new StatusLogEntry(level, message));
            }
        }

        /// <summary>
        ///     Gets the singleton instance of the Logger class.
        /// </summary>
        public static Logger Get()
        {
            return Instance;
        }

        /// <summary>
        ///     Saves a daily log for the specified job and its associated tasks in the given format (JSON or XML).
        /// </summary>
        /// <param name="job">The job whose properties are to be logged.</param>
        /// <param name="tasks">The list of tasks associated with the job.</param>
        /// <param name="format">The format in which the log should be saved (JSON or XML).</param>
        public void SaveDailyLog(List<JobTask> tasks)
        {
            using var writer = new StreamWriter(GetDailyLogFilePath(), true);
            string fileContent;
            switch (DailyLogFormat)
            {
                default:
                case Format.Json:
                    // Pretty print JSON
                    var json = JsonSerialize(tasks);
                    fileContent = json.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    Log(LogLevel.Information, $"Saving daily log to file in JSON format at {GetDailyLogFilePath()}");
                    break;

                case Format.Xml:
                    // Pretty print XML
                    var xmlDoc = new XmlDocument();
                    xmlDoc.AppendChild(xmlDoc.ImportNode(XmlSerialize(tasks), true));
                    using (var stringWriter = new StringWriter())
                    using (var xmlTextWriter = new XmlTextWriter(stringWriter))
                    {
                        xmlTextWriter.Formatting = Formatting.Indented;
                        xmlDoc.WriteTo(xmlTextWriter);
                        fileContent = stringWriter.ToString();
                    }

                    Log(LogLevel.Information, $"Saving daily log to file in XML format at {GetDailyLogFilePath()}");
                    break;
            }

            writer.WriteLine(fileContent);
            writer.WriteLine();
        }


        /// <summary>
        ///     Logs a message to the status log file.
        /// </summary>
        private void LogToFile(StatusLogEntry logEntry)
        {
            File.AppendAllText(GetStatusLogFilePath(),
                $"[{logEntry.Timestamp:HH:mm:ss}][{logEntry.Level.ToString().ToUpper()}]: {logEntry.Message}" +
                Environment.NewLine);
        }

        /// <summary>
        ///     Retrieves the path to the status log file.
        /// </summary>
        public string GetStatusLogFilePath()
        {
            if (!Directory.Exists(StatusLogPath))
                Directory.CreateDirectory(StatusLogPath);

            return Path.Combine(StatusLogPath, $"statusLog-{DateTime.Now:dd-MM-yyyy}.log");
        }

        /// <summary>
        ///     Retrieves the path to the daily log file.
        /// </summary>
        public string GetDailyLogFilePath()
        {
            if (!Directory.Exists(DailyLogPath))
                Directory.CreateDirectory(DailyLogPath);

            return Path.Combine(DailyLogPath, $"dailyLog-{DateTime.Now:dd-MM-yyyy}.log");
        }

        /// <summary>
        ///     Serializes Tasks to JSON format.
        ///     Creates a JSON object with job properties and an array of task objects.
        /// </summary>
        private JsonNode JsonSerialize(List<JobTask> tasks)
        {
            var array = new JsonArray();

            foreach (var jobTask in tasks)
                array.Add(jobTask.JsonSerialize());

            return array;
        }

        /// <summary>
        ///     Serializes the job and its tasks to XML format.
        ///     Creates an XML document with job properties and a list of task elements.
        /// </summary>
        private XmlElement XmlSerialize(List<JobTask> tasks)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("Tasks");

            foreach (var jobTask in tasks)
                root.AppendChild(jobTask.XmlSerialize(doc));

            doc.AppendChild(root);
            return root;
        }
    }

    /// <summary>
    ///     Holds a log entry with its level, message, and timestamp.
    /// </summary>
    public class StatusLogEntry
    {
        public StatusLogEntry(LogLevel level, string message)
        {
            Level = level;
            Message = message;
        }

        public LogLevel Level { get; }
        public string Message { get; }
        public DateTime Timestamp { get; } = DateTime.Now;
    }
}