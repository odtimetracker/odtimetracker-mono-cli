// TODO Chybejici konfiguracni soubor nesmi nicemu branit, proste se bude fungovat bez nej s defaultni lokaci pro Sqlite databazi.
// TODO Pokud je v konfiguracnim souboru uvedena cesta k Sqlite databazi, tak ji musime pouzit.
// TODO V konfiguraci take muze byt nastaveni pro zapnuti/vypnuti barevneho vystupu bez nutnosti pouzit prepinace.

namespace odTimeTracker
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Mono.Data.Sqlite;
	using odTimeTracker.Model;
	using odTimeTracker.Storage;

	class MainClass
	{
		// Flags
		private static bool ColoredOutput = false;

		// Command and its value
		private static string Command;
		private static string CommandValue;

		/// <summary>Instance of used storage.</summary>
		private static SqliteStorage storage;
		public static SqliteStorage Storage {
			get {
				if (storage == null)
				{
					storage = new odTimeTracker.Storage.SqliteStorage();
					storage.Initialize();
				}

				return storage;
			}
		}

		/// <summary>
		/// Asks the yes no question.
		/// </summary>
		/// <returns><c>true</c>, if user entered "y", <c>false</c> otherwise.</returns>
		/// <param name="question">Question to ask.</param>
		private static bool AskYesNoQuestion(string question)
		{
			PrintLine(question + " (y/n)");
			string res = Console.ReadKey().KeyChar.ToString();

			// Move console cursor back left
			Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

			return (res == "y");
		}

		/// <summary>
		/// Prints list of latest activities to the console.
		/// </summary>
		private static void ListActivities()
		{
			List<Activity> activities = Storage.SelectActivities();

			foreach (Activity activity in activities)
			{
				if (ColoredOutput == true)
				{
					Console.ForegroundColor = ConsoleColor.Blue;
					Console.Write("{0}\t", activity.ActivityId.ToString());
					Console.ResetColor();

					Console.ForegroundColor = ConsoleColor.White;
					Console.Write("{0} ", activity.Name);
					Console.ResetColor();

					if (activity.Tags.Trim() != "")
					{
						Console.ForegroundColor = ConsoleColor.Gray;
						Console.Write(" : {0} ", activity.Tags);
						Console.ResetColor();
					}

					if (activity.Description.Trim() != "")
					{
						Console.ForegroundColor = ConsoleColor.Gray;
						Console.Write("({0})", activity.Description);
						Console.ResetColor();
					}
				}
				else
				{
					Console.Write("{0}\t", activity.ActivityId.ToString());
					Console.Write("{0} ", activity.Name);

					if (activity.Tags.Trim() != "")
					{
						Console.Write(" : {0} ", activity.Tags);
					}

					if (activity.Description.Trim() != "")
					{
						Console.Write("({0})", activity.Description);
					}
				}

				Console.Write("\n");
			}
		}

		/// <summary>
		/// Prints list of projects to the console.
		/// </summary>
		private static void ListProjects()
		{
			List<Project> Projects = Storage.SelectProjects();

			foreach (Project project in Projects)
			{
				if (ColoredOutput == true)
				{
					Console.ForegroundColor = ConsoleColor.Blue;
					Console.Write("{0}\t", project.ProjectId.ToString());
					Console.ResetColor();

					Console.ForegroundColor = ConsoleColor.White;
					Console.Write("{0} ", project.Name);
					Console.ResetColor();

					if (project.Description.Trim() != "")
					{
						Console.ForegroundColor = ConsoleColor.Gray;
						Console.Write("({0})", project.Description);
						Console.ResetColor();
					}
				}
				else
				{
					Console.Write("{0}\t", project.ProjectId.ToString());
					Console.Write("{0} ", project.Name);

					if (project.Description.Trim() != "")
					{
						Console.Write("({0})", project.Description);
					}
				}

				Console.Write("\n");
			}
		}

		/// <summary>
		/// Prints today statistics to the console.
		/// </summary>
		private static void ListTodayStats()
		{
			PrintLine("XXX Finish `ListTodayStats()` method!", ConsoleColor.Red, true);
		}

		/// <summary>Prints the line to the console.</summary>
		private static void PrintLineInner(string str, ConsoleColor color, bool newLine)
		{
			if (ColoredOutput == true)
			{
				Console.ForegroundColor = color;
			}

			Console.WriteLine(str);

			if (ColoredOutput == true)
			{
				Console.ResetColor();
			}

			if (newLine == true)
			{
				Console.WriteLine();
			}
		}

		public static void PrintLine(string str)
		{
			PrintLineInner(str, ConsoleColor.White, false);
		}

		public static void PrintLine(string str, bool newLine)
		{
			PrintLineInner(str, ConsoleColor.White, newLine);
		}

		public static void PrintLine(string str, ConsoleColor color)
		{
			PrintLineInner(str, color, false);
		}

		public static void PrintLine(string str, ConsoleColor color, bool newLine)
		{
			PrintLineInner(str, color, newLine);
		}

		/// <summary>
		/// Prints the wrong arguments message.
		/// </summary>
		private static void PrintWrongArgumentsMessage()
		{
			PrintLine("Wrong arguments passed - try `help` argument.", ConsoleColor.Red, true);
		}

		/// <summary>
		/// Prints activity running time.
		/// </summary>
		/// <param name="duration">Duration.</param>
		private static void PrintRunningTime(TimeSpan duration)
		{
			string Output = "Running time:";

			int Hours = duration.Hours;
			if (Hours > 0)
			{
				Output += " " + Hours + " h";
			}

			int Mins = duration.Minutes;
			if (Mins > 0)
			{
				Output += " " + Mins + " m";
			}

			int Secs = duration.Seconds;
			if (Secs > 0)
			{
				Output += " " + Secs + " s";
			}

			PrintLine(Output, ConsoleColor.Green, true);
		}

		/// <summary>
		/// Processes the arguments.
		/// </summary>
		/// <param name="args">Arguments passed to the program.</param>
		private static void ProcessArguments(string[] args)
		{
			foreach (string arg in args)
			{
				// Flag: [-c|--colors]
				if (arg == "-c" || arg == "--colors")
				{
					ColoredOutput = true;
				}
				// Commands: [info|install|help|start|stop]
				else if (
					(arg == "help" || arg == "info" || arg == "list" || arg == "start" || arg == "stop") && 
					(Command == "" || Command == null)
				)
				{
					Command = arg;
				}
				// Get <topic> or <activity>
				else if (Command == "help" || Command == "list" || Command == "start")
				{
					CommandValue = arg;
				}
			}
		}

		//
		// ==================================================================================
		//

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static void Main(string[] args)
		{
			// No arguments - print error message
			if (args.Length == 0 || args.Length > 4)
			{
				PrintWrongArgumentsMessage();
				return;
			}

			// Process arguments
			ProcessArguments(args);

			if ((Command == "list" || Command == "start") && (args.Length < 2 || args.Length > 3))
			{
				PrintWrongArgumentsMessage();
				return;
			}

			// Perform the action self
			switch (Command)
			{
				case "help":
					CmdHelp(CommandValue);
					break;
				case "info":
					CmdInfo();
					break;
				case "list":
					CmdList(CommandValue);
					break;
				case "start":
					CmdStart(CommandValue);
					break;
				case "stop":
					CmdStop();
					break;
				default:
					PrintWrongArgumentsMessage();
					break;
			}
		}

		//
		// ==================================================================================
		//

		/// <summary>
		/// Command `help` - prints application's help.
		/// </summary>
		/// <param name="topic">Help topic.</param>
		private static void CmdHelp(string topic)
		{
			// TODO Finish `help` command (fill all topics)!

			string executable = System.Reflection.Assembly.GetEntryAssembly().Location;
			string appName = Path.GetFileNameWithoutExtension(executable);
			string sample1 = "\"New activity@Project name;tag1,tag2,tag3#Activity description.\"";
			string sample2 = "\"New activity@Project name\"";
			string sample3 = "\"New activity;tag3,tag1#Activity description.\"";
			string sample4 = "\"New activity#Activity description.\"";

			Console.WriteLine();

			switch (topic)
			{
				case "info":
					PrintLine("Description", ConsoleColor.Blue, true);
					PrintLine("Prints info about current status - if is there any activity running prints its name and for how long is running.", true);
					PrintLine("Usage", ConsoleColor.Blue, true);
					PrintLine(" " + appName + " info");
					break;

				case "list":
					PrintLine("Description", ConsoleColor.Blue, true);
					PrintLine("Lists activities, projects or today statistics.", true);
					PrintLine("Usage", ConsoleColor.Blue, true);
					PrintLine(" " + appName + " -c list activities");
					PrintLine(" " + appName + " list projects");
					PrintLine(" " + appName + " list today");
					break;

				case "start":
					PrintLine("Description", ConsoleColor.Blue, true);
					PrintLine("Starts new activity. In one time can exist only one running activity so other activity has to be stopped before the new one is started.", true);
					PrintLine("Activity is described by string where (order is important):", true);
					PrintLine("- the first part is the name of activity");
					PrintLine("- part starting with `@` is name of activity's project");
					PrintLine("- part starting with `;` are comma-separated tags");
					PrintLine("- part starting with `#` is description", true);
					PrintLine("Usage", ConsoleColor.Blue, true);
					PrintLine(" " + appName + " start " + sample1);
					PrintLine(" " + appName + " start " + sample2);
					PrintLine(" " + appName + " start " + sample3);
					PrintLine(" " + appName + " start " + sample4);
					break;

				case "stop":
					PrintLine("Description", ConsoleColor.Blue, true);
					PrintLine("Stops currently running activity.", true);
					PrintLine("Usage", ConsoleColor.Blue, true);
					PrintLine(" " + appName + " stop");
					break;

				case "help":
				default:
					PrintLine("Example Usage", ConsoleColor.Blue, true);
					PrintLine(" " + appName + " start " + sample1);
					PrintLine(" " + appName + " stop");
					PrintLine(" " + appName + " help start", true);
					PrintLine("Commands", ConsoleColor.Blue, true);
					PrintLine(" help [<topic>]   Display help (general or on given topic)");
					PrintLine(" info             Info about current application status");
					PrintLine(" list <what>      List data (activities, today statistics etc.)");
					PrintLine(" start <activity> Start new activity");
					PrintLine(" stop             Stop currently running activity", true);
					PrintLine("Switches", ConsoleColor.Blue, true);
					PrintLine("  --colors|-c     Turn on colored output");
					break;
			}
		}

		/// <summary>
		/// Command `info` - prints information if any activity is running (and for how long time).
		/// </summary>
		private static void CmdInfo()
		{
			var fActivity = Storage.GetRunningActivity();
			if (fActivity[0] == null)
			{
				PrintLine("No activity is running.", ConsoleColor.Magenta, true);
				return;
			}

			Activity runningActivity = fActivity[0];

			PrintLine("There is running activity: " + runningActivity.Name, ConsoleColor.Green);
			PrintRunningTime(runningActivity.GetDuration());
		}

		/// <summary>
		/// Command `list` - prints list of requested data to the console.
		/// Supported data to list: activities, projects, today
		/// </summary>
		/// <param name="what">What data to list.</param>
		private static void CmdList(string what)
		{
			switch (what)
			{
				case "activities":
					ListActivities();
					break;
				case "projects":
					ListProjects();
					break;
				case "today":
					ListTodayStats();
					break;
				default:
					PrintLine("Data keyword '" + what + "' is not recognized. " + 
						"Try help for more informations.", ConsoleColor.Red, true);
					break;
			}
		}

		/// <summary>
		/// Command `start` - starts new activity.
		/// </summary>
		/// <param name="activityString">Activity description.</param> 
		private static void CmdStart(string activityString)
		{
			// Check if there is running activity
			var fActivity = Storage.GetRunningActivity();
			if (fActivity[0] != null)
			{
				PrintLine("Can not create new activity - other activity is still running.", 
					ConsoleColor.Red, true);
				return;
			}

			// Create new activity
			Activity newActivity = new Activity(activityString, Storage);

			// Save it into the database
			newActivity = Storage.InsertActivity(newActivity);

			// Print message and that is all
			PrintLine("New activity was successfully started with ID " + 
				newActivity.ActivityId.ToString() + ".", 
				ConsoleColor.Green, true);
		}

		/// <summary>
		/// Command `stop` - stops currently running activity.
		/// </summary>
		private static void CmdStop()
		{
			var fActivity = Storage.GetRunningActivity();
			if (fActivity[0] == null)
			{
				PrintLine("Can not stop activity - no activity is running.", 
					ConsoleColor.Red, true);
				return;
			}

			Activity runningActivity = Storage.StopActivity(fActivity[0]);

			PrintLine("Activity '" + runningActivity.Name + "' was successfully stopped!", 
				ConsoleColor.Green);
			PrintRunningTime(runningActivity.GetDuration());
		}
	}
}
