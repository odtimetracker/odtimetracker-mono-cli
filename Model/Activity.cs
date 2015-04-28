namespace odTimeTracker
{
	using System;
	using odTimeTracker.Storage;

	namespace Model
	{
		public class Activity
		{
			public long ActivityId { get; set; }
			public long ProjectId { get; set; }
			public string Name { get; set; }
			public string Description { get; set; }
			public string Tags { get; set; }
			public DateTime Created { get; set; }
			public DateTime Stopped { get; set; }

			private DateTime BlankDateTime = DateTime.Parse("0001-01-01 00:00:00.000");

			/// <summary>
			/// Initializes a new instance of the <see cref="odTimeTracker.Model.Activity"/> class 
			/// as a blank activity.
			/// </summary>
			public Activity()
			{
				Created = DateTime.UtcNow;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="odTimeTracker.Model.Activity"/> class 
			/// from string describing the activity.
			/// </summary>
			/// <description>
			/// Here are some examples of valid activity strings:
			/// 
			/// - "Updating README.md@odTimeTracker;Projects#Updating README.md with examples of usage."
			/// - "Simple activity without any values"
			/// - "Activity with project specified@Project name"
			/// - "Activity with some tags;tag1,tag2"
			/// 
			/// Order of values (name, project, tags, description) is important. But the only 
			/// required value is `name`.
			/// </description>
			/// <param name="activityString">Activity string.</param>
			public Activity(string activityString, SqliteStorage storage)
			{
				Console.WriteLine(activityString);
				Console.WriteLine("");

				string name = "";
				string projectName = "";
				string tags = "";
				string description = "";

				string rest = activityString;
				string[] parts;

				bool hasProjectName = (rest.IndexOf('@') >= 0);
				bool hasTags = (rest.IndexOf(';') >= 0);
				bool hasDescription = (rest.IndexOf('#') >= 0);

				// Project name
				if (hasProjectName == true)
				{
					parts = activityString.Split('@');
					name = parts[0];
					rest = parts[1];
				}

				// Tags
				if (hasTags)
				{
					parts = rest.Split(';');

					if (hasProjectName == true)
					{
						projectName = parts[0];
					} else
					{
						name = parts[0];
					}

					rest = parts[1];
				}

				// Description
				if (hasDescription == true)
				{
					parts = rest.Split('#');

					if (hasProjectName == false && hasTags == false)
					{
						name = parts[0];
					} 
					else if (hasProjectName == false && hasTags == true)
					{
						tags = parts[0];
					} 
					else if (hasProjectName == true && hasTags == false)
					{
						projectName = parts[0];
					}
					else /*if (hasProjectName == true && hasTags == true)*/
					{
						tags = parts[0];
					}

					description = parts[1];
				}

				if (hasTags == true && tags.Length == 0)
				{
					tags = rest;
				}
				else if (hasProjectName == true && projectName.Length == 0)
				{
					projectName = rest;
				}
				else if (name.Length == 0)
				{
					name = rest;
				}

				// Check if project with given name exists and if no create it!
				long projectId = 0;
				if (hasProjectName == true && projectName.Length > 0)
				{
					var fProject = storage.SelectProjectByName(projectName);

					if (fProject[0] == null)
					{
						Project newProject = new Project();
						newProject.Name = projectName;
						newProject.Created = DateTime.UtcNow;
						newProject = storage.InsertProject(newProject);
						projectId = newProject.ProjectId;
					}
					else
					{
						projectId = fProject[0].ProjectId;
					}

					ProjectId = projectId;
				}

				ProjectId = projectId;
				Name = name;
				Description = description;
				Tags = tags;
				Created = DateTime.UtcNow;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="odTimeTracker.Model.Activity"/> class 
			/// as a running activity (created Now).
			/// </summary>
			public Activity(long activityId, long projectId, string name, string description, string tags)
			{
				ActivityId = activityId;
				ProjectId = projectId;
				Name = name;
				Description = description;
				Tags = tags;
				Created = DateTime.UtcNow;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="odTimeTracker.Model.Activity"/> class 
			/// as a running activity (created in specified datetime).
			/// </summary>
			public Activity(long activityId, long projectId, string name, string description, string tags, DateTime created)
			{
				ActivityId = activityId;
				ProjectId = projectId;
				Name = name;
				Description = description;
				Tags = tags;
				Created = created;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="odTimeTracker.Model.Activity"/> class.
			/// </summary>
			public Activity(long activityId, long projectId, string name, string description, string tags, DateTime created, DateTime stopped)
			{
				ActivityId = activityId;
				ProjectId = projectId;
				Name = name;
				Description = description;
				Tags = tags;
				Created = created;
				Stopped = stopped;
			}

			/// <summary>
			/// Retrieves activity duration (if activity is not finished 
			/// yet will be returned duration up to the current time).
			/// </summary>
			/// <returns>Duration's activity.</returns>
			public TimeSpan GetDuration()
			{
				DateTime Stopped1 = (Stopped.CompareTo(BlankDateTime) == 0) 
					? DateTime.Now 
					: Stopped;

				return Stopped1.Subtract(Created);
			}

			// XXX Add own ToString() method!
		}
	}
}
