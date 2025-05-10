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

    public void Log(LogLevel level, string message)
    {
        _internalLogger.Log(level, message);
        LogToFile(new StatusLogEntry(message));
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
        using (StreamWriter writer = new StreamWriter(DailyLogPath, true))
        {
            if (format == Format.Json)
            {
                JsonObject json = JsonSerialize();
                writer.WriteLine(json.ToString());
            }
            else if (format == Format.Xml)
            {
                XmlElement xml = XmlSerialize();
                writer.WriteLine(xml.OuterXml);
            }
            writer.WriteLine();
        }
        _jobTasks.Clear();
    }

    public void LogToFile(StatusLogEntry logEntry)
    {
        // Create the directory if it doesn't exist
        if (!Directory.Exists(_statusLogPath))
            Directory.CreateDirectory(_statusLogPath);
        
        File.AppendAllText(GetStatusLogFileName(), $"[{logEntry.Timestamp:HH:mm:ss}]: {logEntry.Message}" + Environment.NewLine);
    }
    
    private string GetStatusLogFileName()
    {
        return Path.Combine(_statusLogPath, $"statusLog-{DateTime.Now:dd-MM-yyyy}.log");
    }
    
    private string GetDailyLogFileName()
    {
        return Path.Combine(_dailyLogPath, $"dailyLog-{DateTime.Now:dd-MM-yyyy}.log");
    }

    public JsonObject JsonSerialize()
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

    public XmlElement XmlSerialize()
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
