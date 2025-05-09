using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.Utilities;
public enum Format
{
    Json,
    Xml
}

public class Logger
{
    private string _dailyLogOutputPath { get; set; }
    private string _statusLogOutputPath { get; set; }


    private List<JobTask> _jobTasks = new List<JobTask>();
    private List<StatusLogEntry> _statusLogEntries = new List<StatusLogEntry>();

    public static readonly Logger Instance = new Logger();

    public static Logger Get()
    {
        return Instance;
    }

    public void AddJob(JobTask task)
    {
        _jobTasks.Add(task);
    }

    public void AddLogEntry(StatusLogEntry logEntry)
    {
        _statusLogEntries.Add(logEntry);
    }

    public string GetDailyLogPath()
    {
        return _dailyLogOutputPath;
    }

    public void SetDailyLogPath(string path)
    {
        _dailyLogOutputPath = path;
    }


    public string GetStatusLogPath()
    {
        return _statusLogOutputPath;
    }

    public void SetStatusLogPath(string path)
    {
        _statusLogOutputPath = path;
    }


    public void DailyLogToFile(Format format)
    {
        using (StreamWriter writer = new StreamWriter(GetDailyLogPath(), true))
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

    public void StatusLogToFile()
    {
        using (StreamWriter writer = new StreamWriter(GetStatusLogPath(), true))
        {
            foreach(StatusLogEntry LogEntry in  _statusLogEntries)
            {
                writer.WriteLine($"[{LogEntry.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")}]: {LogEntry.Message}");
            }
        }
        _statusLogEntries.Clear();
    }

    public JsonObject JsonSerialize()
    {
        JsonObject json = new JsonObject();

        foreach (JobTask JobTask in _jobTasks)
        {
            foreach (Property<dynamic> property in JobTask.GetProperties())
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

        foreach (JobTask JobTask in _jobTasks)
        {
            XmlElement entry = doc.CreateElement("LogEntry");
            foreach (Property<dynamic> property in JobTask.GetProperties())
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
