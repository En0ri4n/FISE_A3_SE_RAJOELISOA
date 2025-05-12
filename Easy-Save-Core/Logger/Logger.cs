using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore.Utilities;

public enum Format
{
    Json,
    Xml
}

/// <summary>
/// Represents a logger that handles logging messages to both daily and status log files.
/// It provides methods to save logs in different formats (JSON or XML).
/// It also implements the singleton pattern to ensure only one instance of the logger exists.
/// </summary>
public class Logger<TJob> where TJob : IJob
{
    private string _dailyLogPath;
    public string DailyLogPath { get => _dailyLogPath; set => _dailyLogPath = value; }
    
    private string _statusLogPath;
    public string StatusLogPath { get => _statusLogPath; set => _statusLogPath = value; }

    private readonly ILogger _internalLogger;
    
    private static readonly Logger<TJob> Instance = new Logger<TJob>();
    
    /// <summary>
    /// Singleton instance of the Logger class.
    /// </summary>
    private Logger()
    {
        _dailyLogPath = @"logs\daily\";
        _statusLogPath = @"logs\status\";
        
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "[HH:mm:ss] ";
        }));
        _internalLogger = factory.CreateLogger(EasySaveCore<TJob>.Name);
    }

    /// <summary>
    /// Logs a message to the status log file.
    /// </summary>
    public static void Log(LogLevel level, string message)
    {
        Instance.LogInternal(level, message);
    }

    public void LogInternal(LogLevel level, string message)
    {
        // _internalLogger.Log(level, message);
        LogToFile(new StatusLogEntry(level, message));
    }

    /// <summary>
    /// Gets the singleton instance of the Logger class.
    /// </summary>
    public static Logger<TJob> Get()
    {
        return Instance;
    }
    
    /// <summary>
    /// Saves a daily log for the specified job and its associated tasks in the given format (JSON or XML).
    /// </summary>
    /// <param name="job">The job whose properties are to be logged.</param>
    /// <param name="tasks">The list of tasks associated with the job.</param>
    /// <param name="format">The format in which the log should be saved (JSON or XML).</param>
    public void SaveDailyLog(IJob job, List<JobTask> tasks, Format format)
    {
        using StreamWriter writer = new StreamWriter(GetDailyLogFilePath(), true);
        string fileContent;
        switch (format)
        {
            default:
            case Format.Json:
                JsonObject json = JsonSerialize(job, tasks);
                fileContent = json.ToString();
                Log(LogLevel.Information, $"Saving daily log to file in JSON format at {GetDailyLogFilePath()}");
                break;
            case Format.Xml:
                XmlElement xml = XmlSerialize(job, tasks);
                fileContent = xml.OuterXml;
                Log(LogLevel.Information, $"Saving daily log to file in XML format at {GetDailyLogFilePath()}");
                break;
        }
        writer.WriteLine(fileContent);
        writer.WriteLine();
    }

    /// <summary>
    /// Logs a message to the status log file.
    /// </summary>
    private void LogToFile(StatusLogEntry logEntry)
    {
        File.AppendAllText(GetStatusLogFilePath(), $"[{logEntry.Timestamp:HH:mm:ss}][{logEntry.Level.ToString().ToUpper()}]: {logEntry.Message}" + Environment.NewLine);
    }
    
    /// <summary>
    /// Retrieves the path to the status log file.
    /// </summary>
    private string GetStatusLogFilePath()
    {
        return Path.Combine(_statusLogPath, $"statusLog-{DateTime.Now:dd-MM-yyyy}.log");
    }
    
    /// <summary>
    /// Retrieves the path to the daily log file.
    /// </summary>
    private string GetDailyLogFilePath()
    {
        return Path.Combine(_dailyLogPath, $"dailyLog-{DateTime.Now:dd-MM-yyyy}.log");
    }

    /// <summary>
    /// Serializes the job and its tasks to JSON format.
    /// Creates a JSON object with job properties and an array of task objects.
    /// </summary>
    private JsonObject JsonSerialize(IJob job, List<JobTask> tasks)
    {
        JsonObject json = new JsonObject();

        // Add job properties to JSON
        foreach (Property<dynamic> prop in job.Properties)
            json.Add(prop.Name, prop.Value);
        
        JsonArray array = new JsonArray();
        
        foreach (JobTask jobTask in tasks)
        {
            JsonObject jsonTask = new JsonObject();
            foreach (Property<dynamic> property in jobTask.GetProperties())
                jsonTask.Add(property.Name, property.Value);
            array.Add(jsonTask);
        }
        
        json.Add("tasks", array);
        
        return json;
    }

    /// <summary>
    /// Serializes the job and its tasks to XML format.
    /// Creates an XML document with job properties and a list of task elements.
    /// </summary>
    private XmlElement XmlSerialize(IJob job, List<JobTask> tasks)
    {
        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("root");
        
        // Add job properties to XML
        foreach (Property<dynamic> prop in job.Properties)
        {
            XmlElement propElement = doc.CreateElement(prop.Name);
            propElement.InnerText = prop.Value.ToString() ?? string.Empty;
            root.AppendChild(propElement);
        }
        
        // Add tasks to XML
        XmlElement tasksElement = doc.CreateElement("Tasks");

        foreach (JobTask jobTask in tasks)
        {
            XmlElement entry = doc.CreateElement("Task");
            foreach (Property<dynamic> property in jobTask.GetProperties())
            {
                XmlElement propElement = doc.CreateElement(property.Name);
                propElement.InnerText = property.Value.ToString() ?? string.Empty;
                entry.AppendChild(propElement);
            }
            tasksElement.AppendChild(entry);
        }
        root.AppendChild(tasksElement);

        doc.AppendChild(root);
        return root;
    }
}

/// <summary>
/// Holds a log entry with its level, message, and timestamp.
/// </summary>
public class StatusLogEntry(LogLevel level, string message)
{
    public LogLevel Level { get; } = level;
    public string Message { get; } = message;
    public DateTime Timestamp { get; } = DateTime.Now;
}
