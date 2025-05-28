using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Translations;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.View;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using static CLEA.EasySaveCore.Models.JobExecutionStrategy;

namespace CLEA.EasySaveCLI
{
    public sealed class EasySaveCli : EasySaveView
    {
        enum Menu
        {
            Main,
            Job,
            JobList,
            RunJob,
            RunMultipleJobs,
            RunAllJobs,
            CreateJob,
            ModifyJob,
            DeleteJob,
            Language,
            LogType,
            DailyLogDirectory,
            StatusLogDirectory,
            Settings
        }

        private readonly List<Menu> _menuHistory = new List<Menu>();
        private readonly BackupJobViewModel _viewModel;

        public EasySaveCli(EasySaveViewModelBase viewModel, JobManager jobManager, EasySaveConfigurationBase configuration) : base(
            EasySaveCore.Core.EasySaveCore.Init(viewModel, jobManager, configuration),
            new BackupJobViewModel(configuration),
            new ViewModelBackupJobBuilder((BackupJobManager) jobManager))
        {
            _viewModel = (BackupJobViewModel)viewModel;
            Console.Title = L10N.GetTranslation("main.title");
            AddToMenuHistory(Menu.Main);
            DisplayMainMenu();
        }

        private void AddToMenuHistory(Menu menuName)
        {
            _menuHistory.Add(menuName);
        }

        protected override void DisplayMainMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText(L10N.GetTranslation("main.title")).Color(Color.Green3).Centered());
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("main_menu.title"))
                    .AddChoices(
                        L10N.GetTranslation("main_menu.jobs"),
                        L10N.GetTranslation("main_menu.settings"),
                        L10N.GetTranslation("main_menu.exit")
                    ));

            if (choice == L10N.GetTranslation("main_menu.jobs"))
            {
                AddToMenuHistory(Menu.Job);
                DisplayJobMenu();
            }
            else if (choice == L10N.GetTranslation("main_menu.settings"))
            {
                AddToMenuHistory(Menu.Settings);
                DisplaySettingsMenu();
            }
            else if (choice == L10N.GetTranslation("main_menu.exit"))
            {
                Exit();
            }
            /*else if (choice == "test_file_explorer")
            {
                FileBrowser.Browser browser = new FileBrowser.Browser();
                browser.GetFolderPath().RunSynchronously();
            }*/
        }

        protected override void DisplayJobMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Text(L10N.GetTranslation("job_menu.title")).Centered());

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .AddChoices(
                        L10N.GetTranslation("job_menu.list_job"),
                        L10N.GetTranslation("job_menu.run_job"),
                        L10N.GetTranslation("job_menu.run_multiple_jobs"),
                        L10N.GetTranslation("job_menu.run_all_jobs"),
                        L10N.GetTranslation("job_menu.create_job"),
                        L10N.GetTranslation("job_menu.modify_job"),
                        L10N.GetTranslation("job_menu.delete_job"),
                        L10N.GetTranslation("main.go_back")
                    ));
            if (choice == L10N.GetTranslation("job_menu.list_job"))
            {
                AddToMenuHistory(Menu.JobList);
                DisplayJobListMenu();
            }
            else if (choice == L10N.GetTranslation("job_menu.run_job"))
            {
                AddToMenuHistory(Menu.RunJob);
                DisplayRunMenu();
            }
            else if (choice == L10N.GetTranslation("job_menu.run_multiple_jobs"))
            {
                AddToMenuHistory(Menu.RunMultipleJobs);
                DisplayRunMultipleMenu();
            }
            else if (choice == L10N.GetTranslation("job_menu.run_all_jobs"))
            {
                AddToMenuHistory(Menu.RunAllJobs);
                DisplayRunAllMenu();
            }
            else if (choice == L10N.GetTranslation("job_menu.create_job"))
            {
                AddToMenuHistory(Menu.CreateJob);
                DisplayCreateJobMenu();
            }
            else if (choice == L10N.GetTranslation("job_menu.modify_job"))
            {
                AddToMenuHistory(Menu.ModifyJob);
                DisplayModifyJobMenu();
            }
            else if (choice == L10N.GetTranslation("job_menu.delete_job"))
            {
                AddToMenuHistory(Menu.DeleteJob);
                DisplayDeleteJobMenu();
            }
            else if (choice == L10N.GetTranslation("main.go_back"))
            {
                GoBack();
            }
        }

        protected override void DisplayLanguageMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("language_menu.title"))
                    .AddChoices(
                        _viewModel.AvailableLanguages.Select(l => l.Name).ToArray()
                            .Append(L10N.GetTranslation("main.go_back"))
                    ));
            if (choice != L10N.GetTranslation("main.go_back"))
            {
                LangIdentifier selectedLang = Languages.SupportedLangs.First(li => li.Name == choice);

                if (selectedLang != L10N.GetLanguage())
                {
                    _viewModel.CurrentApplicationLang = selectedLang;
                }
            }

            GoBack();
        }

        protected override void DisplayLogTypeMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("logtype_menu.title").Replace("{LOGTYPE}",
                        Logger.Get().DailyLogFormat.ToString()))
                    .AddChoices(
                        _viewModel.AvailableDailyLogFormats.Select(f => f.ToString())
                            .Append(L10N.GetTranslation("main.go_back"))
                    ));

            if (choice != L10N.GetTranslation("main.go_back"))
            {
                _viewModel.CurrentDailyLogFormat = Enum.Parse<Format>(choice);
            }

            GoBack();
        }

        protected override void DisplayJobListMenu()
        {
            AnsiConsole.Write(new Text(L10N.GetTranslation("job_menu.list_job")).Centered());
            Table table = new Table();

            table.AddColumns(L10N.GetTranslation("job_menu.column.name"), L10N.GetTranslation("job_menu.column.source"),
                L10N.GetTranslation("job_menu.column.target"));

            foreach (IJob job in _viewModel.JobManager.GetJobs())
                table.AddRow(job.Name, job.Source, job.Target);
            AnsiConsole.Write(table);
            AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
            Console.ReadKey();
            GoBack();
        }

        private bool DisplayPromptRunStrategy()
        {
            AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("run_strategy_menu.title"))
                    .AddChoices(
                        L10N.GetTranslation("run_strategy_menu.option_full"),
                        L10N.GetTranslation("run_strategy_menu.option_differential"),
                        L10N.GetTranslation("main.go_back")
                    ));
            if (choice != L10N.GetTranslation("main.go_back"))
            {
                _viewModel.ChangeRunStrategyCommand.Execute(
                    choice == L10N.GetTranslation("run_strategy_menu.option_full")
                        ? nameof(StrategyType.Full)
                        : nameof(StrategyType.Differential));
                return true;
            }

            return false;
        }

        protected override void DisplayRunMenu()
        {
            string jobName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("job_menu.title_one"))
                    .AddChoices(
                        _viewModel.JobManager.GetJobs().Select(job => job.Name).ToArray()
                            .Append(L10N.GetTranslation("main.go_back"))
                    ));
            if (jobName != L10N.GetTranslation("main.go_back") && DisplayPromptRunStrategy())
            {
                if (!_viewModel.DoesDirectoryPathExist(_viewModel.JobManager.GetJob(jobName).Source))
                {
                    ShowErrorScreen(L10N.GetTranslation("error.path_with").Replace("{JOBNAME}", jobName)
                        .Replace("{PATH}", _viewModel.JobManager.GetJob(jobName).Source));
                    return;
                }

                SetRunHandler(new string[] { jobName });
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Aesthetic)
                    .SpinnerStyle(Style.Parse("blue"))
                    .Start(L10N.GetTranslation("loader.running.text"),
                        ctx => _viewModel.RunJobCommand.Execute(jobName));
                DisplayJobResultMenu(1);
            }

            GoBack();
        }

        protected override void DisplayRunMultipleMenu()
        {
            if (_viewModel.JobManager.JobCount == 0)
            {
                ShowErrorScreen(L10N.GetTranslation("job_menu.error_no_jobs"));
                return;
            }

            List<string> jobListName = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title(L10N.GetTranslation("job_menu.title_multiple"))
                    .NotRequired()
                    .AddChoices(
                        _viewModel.JobManager.GetJobs().Select(job => job.Name).ToArray()
                    ));
            if (jobListName.Count() != 0 && DisplayPromptRunStrategy())
            {
                foreach (string jobName in jobListName)
                {
                    if (!_viewModel.DoesDirectoryPathExist(_viewModel.JobManager.GetJob(jobName).Source))
                    {
                        ShowErrorScreen(L10N.GetTranslation("error.path_with").Replace("{JOBNAME}", jobName)
                            .Replace("{PATH}", _viewModel.JobManager.GetJob(jobName).Source));
                        return;
                    }
                }

                SetRunHandler(jobListName.ToArray());
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Grenade)
                    .SpinnerStyle(Style.Parse("yellow"))
                    .Start(L10N.GetTranslation("loader.running.text"),
                        ctx => _viewModel.RunMultipleJobsCommand.Execute(jobListName));
                DisplayJobResultMenu(jobListName.Count());
            }

            GoBack();
        }

        private void SetRunHandler(string[] jobNames)
        {
            _viewModel.OnTaskCompletedFor(jobNames, task =>
            {
                BackupJobTask backupTask = (BackupJobTask)task;
                AnsiConsole.WriteLine(L10N.GetTranslation("job_menu.task_run_information")
                    .Replace("{JOB_NAME}", backupTask.Name)
                    .Replace("{SOURCE}", backupTask.Source)
                    .Replace("{TARGET}", backupTask.Target)
                    .Replace("{TIME}", backupTask.TransferTime.ToString())
                    .Replace("{STATUS}", backupTask.Status.ToString()));
            });
        }

        protected override void DisplayRunAllMenu()
        {
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("job_menu.title_all"))
                    .AddChoices(
                        L10N.GetTranslation("job_menu.option_all"),
                        L10N.GetTranslation("main.go_back")
                    ));
            if (choice != L10N.GetTranslation("main.go_back") && DisplayPromptRunStrategy())
            {
                foreach (IJob job in _viewModel.JobManager.GetJobs())
                {
                    if (!_viewModel.DoesDirectoryPathExist(job.Source))
                    {
                        ShowErrorScreen(L10N.GetTranslation("error.path_with").Replace("{JOBNAME}", job.Name)
                            .Replace("{PATH}", job.Source));
                        return;
                    }
                }

                SetRunHandler(_viewModel.AvailableJobs.Select(x => x.Name).ToArray());
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Pong)
                    .SpinnerStyle(Style.Parse("green"))
                    .Start(L10N.GetTranslation("loader.running.text"),
                        ctx => { _viewModel.RunAllJobsCommand.Execute(null); });
                DisplayJobResultMenu(_viewModel.JobManager.GetJobs().Count());
            }

            GoBack();
        }

        protected override void DisplayCreateJobMenu()
        {
            if (_viewModel.JobManager.JobCount == 5)
            {
                ShowErrorScreen(L10N.GetTranslation("job_menu.error_excessive_jobs"));
                return;
            }

            _viewModel.JobBuilderBase.Name = AnsiConsole.Ask<string>(L10N.GetTranslation("job_menu.name_question"));
            if (!_viewModel.IsNameValid(_viewModel.JobBuilderBase.Name, true))
            {
                ShowErrorScreen(L10N.GetTranslation("job_menu.error_job_exist"));
                return;
            }

            _viewModel.JobBuilderBase.Source = AnsiConsole.Ask<string>(L10N.GetTranslation("information.source_directory"));
            if (!_viewModel.DoesDirectoryPathExist(_viewModel.JobBuilderBase.Source))
            {
                ShowErrorScreen(L10N.GetTranslation("error.path"));
                return;
            }

            _viewModel.JobBuilderBase.Target = AnsiConsole.Ask<string>(L10N.GetTranslation("information.target_directory"));
            if (!_viewModel.IsDirectoryPathValid(_viewModel.JobBuilderBase.Target))
            {
                ShowErrorScreen(L10N.GetTranslation("error.path"));
                return;
            }

            _viewModel.BuildJobCommand.Execute(null);
            GoBack();
        }

        protected override void DisplayModifyJobMenu()
        {
            string jobName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("job_modify_menu.title"))
                    .AddChoices(
                        _viewModel.JobManager.GetJobs().Select(job => job.Name).ToArray()
                            .Append(L10N.GetTranslation("main.go_back"))
                    ));
            AnsiConsole.Clear();
            if (jobName != L10N.GetTranslation("main.go_back"))
            {
                AnsiConsole.Write(new Text(L10N.GetTranslation("job_modify_menu.title_modify_page")).Centered());
                AnsiConsole.WriteLine("‎‎‎ "); // some space to breathe ...
                _viewModel.LoadJobInBuilderCommand.Execute(jobName);
                _viewModel.JobBuilderBase.Name =
                    AnsiConsole.Ask(L10N.GetTranslation("job_menu.name_question"), _viewModel.JobBuilderBase.Name);
                if (!_viewModel.IsNameValid(_viewModel.JobBuilderBase.InitialName, false))
                {
                    ShowErrorScreen(L10N.GetTranslation("job_menu.error_job_exist"));
                    return;
                }

                _viewModel.JobBuilderBase.Source = AnsiConsole.Ask(L10N.GetTranslation("information.source_directory"),
                    _viewModel.JobBuilderBase.Source);
                if (!_viewModel.DoesDirectoryPathExist(_viewModel.JobBuilderBase.Source))
                {
                    ShowErrorScreen(L10N.GetTranslation("error.path"));
                    return;
                }

                _viewModel.JobBuilderBase.Target = AnsiConsole.Ask(L10N.GetTranslation("information.target_directory"),
                    _viewModel.JobBuilderBase.Target);
                if (!_viewModel.IsDirectoryPathValid(_viewModel.JobBuilderBase.Target))
                {
                    ShowErrorScreen(L10N.GetTranslation("error.path"));
                    return;
                }

                _viewModel.UpdateFromJobBuilder();
                GoBack();
            }

            GoBack();
        }

        protected override void DisplayDeleteJobMenu()
        {
            string jobName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which job do you want to delete?")
                    .AddChoices(
                        _viewModel.JobManager.GetJobs().Select(job => job.Name).ToArray()
                            .Append(L10N.GetTranslation("main.go_back"))
                    ));
            if (jobName != L10N.GetTranslation("main.go_back"))
            {
                _viewModel.DeleteJobCommand.Execute(jobName);
            }

            GoBack();
        }

        protected override void DisplayJobResultMenu(int jobNumber)
        {
            AnsiConsole.Clear();
            if (jobNumber == 1)
            {
                AnsiConsole.WriteLine(L10N.GetTranslation("job_menu.one_job_success"));
            }
            else
            {
                AnsiConsole.WriteLine(L10N.GetTranslation("job_menu.many_jobs_success")
                    .Replace("{JOBNUMBER}", jobNumber.ToString()));
            }

            AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
            Console.ReadKey();
        }

        protected override void DisplayDailyLogDirectoryMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

            string path = AnsiConsole.Ask(L10N.GetTranslation("settings_menu.message_daily_log_path"),
                Logger.Get().DailyLogPath);

            if (!_viewModel.IsDirectoryPathValid(path))
            {
                AnsiConsole.WriteLine(L10N.GetTranslation("error.path"));
                AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
                Console.ReadKey();
                GoBack();
                return;
            }

            _viewModel.DailyLogPath = path;

            GoBack();
        }

        protected override void DisplayStatusLogDirectoryMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

            string path = AnsiConsole.Ask(L10N.GetTranslation("settings_menu.message_status_log_path"),
                Logger.Get().StatusLogPath);

            if (!_viewModel.IsDirectoryPathValid(path))
            {
                AnsiConsole.WriteLine(L10N.GetTranslation("error.path"));
                AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
                Console.ReadKey();
                GoBack();
                return;
            }

            _viewModel.StatusLogPath = path;

            GoBack();
        }

        protected override void DisplaySettingsMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());
            string choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(L10N.GetTranslation("settings_menu.title"))
                    .AddChoices(
                        L10N.GetTranslation("settings_menu.change_language"),
                        L10N.GetTranslation("settings_menu.change_log_type").Replace("{LOGTYPE}",
                            Logger.Get().DailyLogFormat.ToString()),
                        L10N.GetTranslation("settings_menu.change_daily_log_path").Replace("{PATH}",
                            Logger.Get().DailyLogPath),
                        L10N.GetTranslation("settings_menu.change_status_log_path").Replace("{PATH}",
                            Logger.Get().StatusLogPath),
                        L10N.GetTranslation("main.go_back")
                    ));

            if (choice == L10N.GetTranslation("settings_menu.change_language"))
            {
                AddToMenuHistory(Menu.Language);
                DisplayLanguageMenu();
            }
            else if (choice == L10N.GetTranslation("settings_menu.change_log_type").Replace("{LOGTYPE}",
                         Logger.Get().DailyLogFormat.ToString()))
            {
                AddToMenuHistory(Menu.LogType);
                DisplayLogTypeMenu();
            }
            else if (choice == L10N.GetTranslation("settings_menu.change_daily_log_path")
                         .Replace("{PATH}", Logger.Get().DailyLogPath))
            {
                AddToMenuHistory(Menu.DailyLogDirectory);
                DisplayDailyLogDirectoryMenu();
            }
            else if (choice == L10N.GetTranslation("settings_menu.change_status_log_path")
                         .Replace("{PATH}", Logger.Get().StatusLogPath))
            {
                AddToMenuHistory(Menu.StatusLogDirectory);
                DisplayStatusLogDirectoryMenu();
            }

            GoBack();
        }

        private void Exit()
        {
            AnsiConsole.Write(L10N.GetTranslation("main.exiting"));
            Thread.Sleep(1000);
            Logger.Log(LogLevel.Information, "Quitting EasySave-CLEA..." + Environment.NewLine);
            AnsiConsole.Clear();
            Environment.Exit(0);
        }

        private void ShowErrorScreen(string error)
        {
            AnsiConsole.Write(new Markup($"[red]{error}[/]"));
            AnsiConsole.Write(Environment.NewLine + L10N.GetTranslation("main.click_any"));
            Console.ReadKey();
            GoBack();
        }

        /// <summary>
        /// Remove current menu from the menu History and go to the one before
        /// Note that only menus with submenus are possible values for the switch case
        /// </summary>
        private void GoBack()
        {
            _menuHistory.RemoveAt(_menuHistory.Count - 1);
            Menu target = _menuHistory.Last();
            switch (target)
            {
                case Menu.Main:
                    DisplayMainMenu();
                    break;
                case Menu.Job:
                    DisplayJobMenu();
                    break;
                case Menu.Settings:
                    DisplaySettingsMenu();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            BackupJobManager backupJobManager = new BackupJobManager();
            BackupJobConfiguration configuration = new BackupJobConfiguration();
            BackupJobViewModel viewModel = new BackupJobViewModel(configuration);
            new EasySaveCli(
                viewModel,
                backupJobManager,
                configuration
            );
        }
    }
}