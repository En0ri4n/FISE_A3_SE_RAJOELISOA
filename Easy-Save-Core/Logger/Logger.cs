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

public class Logger
{
    private string _dailyLogPath;
    public string DailyLogPath { get => _dailyLogPath; set => _dailyLogPath = value; }
    
    private string _statusLogPath;
    public string StatusLogPath { get => _statusLogPath; set => _statusLogPath = value; }

    private readonly ILogger _internalLogger;

    private readonly List<JobTask> _jobTasks;

    private static readonly Logger Instance = new Logger();
    
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
        _internalLogger = factory.CreateLogger(EasySaveCore.Name);

        _jobTasks = [];
    }

    public static void Log(LogLevel level, string message)
    {
        Instance._internalLogger.Log(level, message);
        Instance.LogToFile(new StatusLogEntry(level, message));
    }

    public static Logger Get()
    {
        return Instance;
    }

    public void AddJob(JobTask task)
    {
        _jobTasks.Add(task);
    }
    
    public void SaveDailyLog(Format format)
    {
        using (StreamWriter writer = new StreamWriter(GetDailyLogFilePath(), true))
        {
            string fileContent;
            switch (format)
            {
                default:
                case Format.Json:
                    JsonObject json = JsonSerialize();
                    fileContent = json.ToString();
                    Log(LogLevel.Information, $"Saving daily log to file in JSON format at {GetDailyLogFilePath()}");
                    break;
                case Format.Xml:
                    XmlElement xml = XmlSerialize();
                    fileContent = xml.OuterXml;
                    Log(LogLevel.Information, $"Saving daily log to file in XML format at {GetDailyLogFilePath()}");
                    break;
            }
            writer.WriteLine(fileContent);
            writer.WriteLine();
        }
        _jobTasks.Clear();
    }

    private void LogToFile(StatusLogEntry logEntry)
    {
        File.AppendAllText(GetStatusLogFilePath(), $"[{logEntry.Timestamp:HH:mm:ss}][{logEntry.Level.ToString().ToUpper()}]: {logEntry.Message}" + Environment.NewLine);
    }
    
    private string GetStatusLogFilePath()
    {
        return Path.Combine(_statusLogPath, $"statusLog-{DateTime.Now:dd-MM-yyyy}.log");
    }
    
    private string GetDailyLogFilePath()
    {
        return Path.Combine(_dailyLogPath, $"dailyLog-{DateTime.Now:dd-MM-yyyy}.log");
    }

    private JsonObject JsonSerialize()
    {
        JsonObject json = new JsonObject();

        foreach (JobTask jobTask in _jobTasks)
        {
            foreach (Property<dynamic> property in jobTask.GetProperties())
            {
                json[property.Name] = property.Value?.ToString();
            }
        }
        return json;
    }

    private XmlElement XmlSerialize()
    {
        XmlDocument doc = new XmlDocument();
        XmlElement root = doc.CreateElement("LogEntries");

        foreach (JobTask jobTask in _jobTasks)
        {
            XmlElement entry = doc.CreateElement("LogEntry");
            foreach (Property<dynamic> property in jobTask.GetProperties())
            {
                XmlElement propElement = doc.CreateElement(property.Name);
                propElement.InnerText = property.Value?.ToString() ?? string.Empty;
                entry.AppendChild(propElement);
            }
            root.AppendChild(entry);
        }

        doc.AppendChild(root);
        return root;
    }
}

public class StatusLogEntry(LogLevel level, string message)
{
    public LogLevel Level { get; private set; } = level;
    public string Message { get; private set; } = message;
    public DateTime Timestamp { get; private set; } = DateTime.Now;
}
