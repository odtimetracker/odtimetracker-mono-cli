namespace odTimeTracker
{
	using System;
	using NUnit.Framework;
	using odTimeTracker.Storage;

	namespace Model
	{
		[TestFixture()]
		public class ActivityTest
		{
			[Test]
			public void BlankActivityTest()
			{
				Activity BlankActivity = new Activity();

				Assert.AreEqual(0, BlankActivity.ActivityId);
				Assert.AreEqual(0, BlankActivity.ProjectId);
				Assert.IsNull(BlankActivity.Name);
				Assert.IsNull(BlankActivity.Description);
				Assert.IsNull(BlankActivity.Tags);
				DateTime TestDateTime = DateTime.UtcNow;
				Assert.AreEqual(TestDateTime.Year, BlankActivity.Created.Year);
				Assert.AreEqual(TestDateTime.Month, BlankActivity.Created.Month);
				Assert.AreEqual(TestDateTime.Day, BlankActivity.Created.Day);
				Assert.AreEqual(TestDateTime.Hour, BlankActivity.Created.Hour);
				Assert.AreEqual(DateTime.Parse("0001-01-01 00:00:00.000"), BlankActivity.Stopped);
			}

			[Test]
			public void FinishedActivityTest()
			{
				Activity FinishedActivity = new Activity(1, 1,
					"Sample activity",
					"Description of the sample activity.",
					"tag1,tag2",
					DateTime.Parse("2015-01-01 01:00:00.000"),
					DateTime.Parse("2015-01-01 02:00:00.000"));

				Assert.AreEqual(1, FinishedActivity.ActivityId);
				Assert.AreEqual(1, FinishedActivity.ProjectId);
				Assert.AreEqual("Sample activity", FinishedActivity.Name);
				Assert.AreEqual("Description of the sample activity.", FinishedActivity.Description);
				Assert.AreEqual("tag1,tag2", FinishedActivity.Tags);
				Assert.AreEqual(DateTime.Parse("2015-01-01 01:00:00.000"), FinishedActivity.Created);
				Assert.AreEqual(DateTime.Parse("2015-01-01 02:00:00.000"), FinishedActivity.Stopped);
				Assert.AreEqual(3600, FinishedActivity.GetDuration().TotalSeconds);
			}

			[Test]
			public void RunningActivityTest()
			{
				Activity RunningActivity = new Activity(2, 1,
					"Sample running activity",
					"Some description...", null,
					DateTime.Parse("2015-01-01 02:15:00.000"));

				Assert.AreEqual(2, RunningActivity.ActivityId);
				Assert.AreEqual(1, RunningActivity.ProjectId);
				Assert.AreEqual("Sample running activity", RunningActivity.Name);
				Assert.AreEqual("Some description...", RunningActivity.Description);
				Assert.IsNull(RunningActivity.Tags);
				Assert.AreEqual(DateTime.Parse("2015-01-01 02:15:00.000"), RunningActivity.Created);
				Assert.AreEqual(DateTime.Parse("0001-01-01 00:00:00.000"), RunningActivity.Stopped);
			}

			[Test]
			public void ActivityFromStringTest()
			{
				SqliteStorage Storage = new SqliteStorage("Data Source=:memory:");
				Storage.Initialize();

				Activity act1 = new Activity("Test activity 1", Storage);
				Assert.AreEqual(0, act1.ActivityId);
				Assert.AreEqual("Test activity 1", act1.Name);
				Assert.AreEqual("", act1.Description);
				Assert.AreEqual("", act1.Tags);

				Activity act2 = new Activity("Test activity 2@Project name", Storage);
				Assert.AreEqual("Test activity 2", act2.Name);
				Assert.AreEqual("", act2.Description);
				Assert.AreEqual("", act2.Tags);

				Activity act3 = new Activity("Test activity 3@Project name;tag1,tag2", Storage);
				Assert.AreEqual("Test activity 3", act3.Name);
				Assert.AreEqual("", act3.Description);
				Assert.AreEqual("tag1,tag2", act3.Tags);

				Activity act4 = new Activity("Test activity 4@Project name;tag1,tag2,tag3#Some description", Storage);
				Assert.AreEqual("Test activity 4", act4.Name);
				Assert.AreEqual("Some description", act4.Description);
				Assert.AreEqual("tag1,tag2,tag3", act4.Tags);

				Activity act5 = new Activity("Test activity 5;tag1,tag2", Storage);
				Assert.AreEqual("Test activity 5", act5.Name);
				Assert.AreEqual("", act5.Description);
				Assert.AreEqual("tag1,tag2", act5.Tags);

				Activity act6 = new Activity("Test activity 6;tag3#Some description", Storage);
				Assert.AreEqual("Test activity 6", act6.Name);
				Assert.AreEqual("Some description", act6.Description);
				Assert.AreEqual("tag3", act6.Tags);

				Activity act7 = new Activity("Test activity 7@Project name#Some description", Storage);
				Assert.AreEqual("Test activity 7", act7.Name);
				Assert.AreEqual("Some description", act7.Description);
				Assert.AreEqual("", act7.Tags);

				Activity act8 = new Activity("Test activity 8#Some description", Storage);
				Assert.AreEqual("Test activity 8", act8.Name);
				Assert.AreEqual("Some description", act8.Description);
				Assert.AreEqual("", act8.Tags);
			}
		}
	}
}
