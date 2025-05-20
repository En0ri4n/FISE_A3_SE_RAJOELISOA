using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Models;
using EasySaveCore.Models;
using Microsoft.Extensions.Logging;

namespace CLEA_Tests;

public class LoggerTests
{
    // [SkippableFact]
    // public void Log_ShouldWriteMessageToStatusLogFile()
    // {
    //     Skip.IfNot(EasySaveCore<IJob>.Version.Major == 1);
    //     
    //     // Arrange
    //     CLEA.EasySaveCore.Utilities.Logger logger = CLEA.EasySaveCore.Utilities.Logger.Get();
    //     string testMessage = "Test log message";
    //     LogLevel testLevel = LogLevel.Information;
    //
    //     // Act
    //     CLEA.EasySaveCore.Utilities.Logger.Log(testLevel, testMessage);
    //
    //     // Assert
    //     string logFilePath = Directory.GetFiles(logger.StatusLogPath, $"statusLog-{DateTime.Now:dd-MM-yyyy}.log")[0];
    //     string logContent = File.ReadAllText(logFilePath);
    //     Assert.Contains(testMessage, logContent);
    //     Assert.Contains(testLevel.ToString().ToUpper(), logContent);
    // }
    
    // [SkippableFact]
    // public void SaveDailyLog_ShouldCreateJsonLogFile()
    // {
    //     Skip.IfNot(EasySaveCore.Version.Major == 1);
    //     
    //     // Arrange
    //     Logger logger = Logger.Get();
    //     IJob mockJob = new MockJob();
    //     List<JobTask> mockTasks = new List<JobTask>
    //     {
    //         new JobTask { Name = "Task1", Status = "Completed" }
    //     };
    //
    //     // Act
    //     logger.SaveDailyLog(mockJob, mockTasks, Format.Json);
    //
    //     // Assert
    //     string logFilePath = Directory.GetFiles(logger.DailyLogPath, "dailyLog-*.log")[0];
    //     string logContent = File.ReadAllText(logFilePath);
    //     Assert.Contains("\"Name\": \"Task1\"", logContent);
    //     Assert.Contains("\"Status\": \"Completed\"", logContent);
    // }
    //
    // [SkippableFact]
    // public void SaveDailyLog_ShouldCreateXmlLogFile()
    // {
    //     Skip.IfNot(EasySaveCore.Version.Major == 1);
    //
    //     // Arrange
    //     Logger logger = Logger.Get();
    //     IJob mockJob = new MockJob();
    //     List<JobTask> mockTasks = new List<JobTask>
    //     {
    //         new JobTask { Name = "Task1", Status = "Completed" }
    //     };
    //
    //     // Act
    //     logger.SaveDailyLog(mockJob, mockTasks, Format.Xml);
    //
    //     // Assert
    //     string logFilePath = Directory.GetFiles(logger.DailyLogPath, "dailyLog-*.log")[0];
    //     string logContent = File.ReadAllText(logFilePath);
    //     Assert.Contains("<Name>Task1</Name>", logContent);
    //     Assert.Contains("<Status>Completed</Status>", logContent);
    // }
    //
    // [SkippableFact]
    // public void JsonSerialize_ShouldReturnCorrectJsonObject()
    // {
    //     Skip.IfNot(EasySaveCore.Version.Major == 1);
    //
    //     // Arrange
    //     Logger logger = Logger.Get();
    //     IJob mockJob = new MockJob();
    //     List<JobTask> mockTasks = new List<JobTask>
    //     {
    //         new JobTask { Name = "Task1", Status = "Completed" }
    //     };
    //
    //     // Act
    //     JsonObject json = logger.JsonSerialize(mockJob, mockTasks);
    //
    //     // Assert
    //     Assert.Equal("Task1", json["tasks"]?[0]?["Name"]?.ToString());
    //     Assert.Equal("Completed", json["tasks"]?[0]?["Status"]?.ToString());
    // }
    //
    // [SkippableFact]
    // public void XmlSerialize_ShouldReturnCorrectXmlElement()
    // {
    //     Skip.IfNot(EasySaveCore.Version.Major == 1);
    //
    //     // Arrange
    //     Logger logger = Logger.Get();
    //     IJob mockJob = new MockJob();
    //     List<JobTask> mockTasks = new List<JobTask>
    //     {
    //         new JobTask { Name = "Task1", Status = "Completed" }
    //     };
    //
    //     // Act
    //     XmlElement xml = logger.XmlSerialize(mockJob, mockTasks);
    //
    //     // Assert
    //     Assert.NotNull(xml.SelectSingleNode("//Name[text()='Task1']"));
    //     Assert.NotNull(xml.SelectSingleNode("//Status[text()='Completed']"));
    // }
}