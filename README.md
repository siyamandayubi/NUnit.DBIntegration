# NUnit.DBIntegration
The project extends the NUnit to do integration tests involving database easily. 
It lets you to backup/restore database & run custom scripts for each test.
# How to use it
Using it simple!.
First, you need to add the following keys to the app.config of your testing project.
```xml
    <add key="DatabaseName" value="[DBNAME]" />
    <add key="BackupPath" value="[ADDRESS OF THE BACKUP FILE]" />
    <add key="ScriptFilesPath" value="[ADDRESS OF A DIRECTORY THAT CONTAINS THE SCRIPT FILES THAT MUST BE RUN ON THE RESTORED DB]" />
    <add key="AfterDeploymentScriptFilePath" value="[ADDRESS OF THE SCRIPT FILE THAT WILL RUN ON THE MASTER DB. (Use it for creating custom user etc.]" />
    <add key="ScriptFilter" value="*.sql" />
```

Then you need to initialize the database.
``` c#
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void Initialize()
        {
            NUnit.DBIntegration.DatabaseWrapper.Initialize(DatabaseTypes.SqlServer);
        }
    }
```

And finally you can have test methods.
``` C#
       [Test]
        [DBTest(scriptFiles: new[] { "ADDRESS OF SCRIPT FILE THAT PREPARE DB FOR THIS TEST, Empty in case of having no such files" }, resetDatabase: true)]
        public void Test1()
        {
        }

```

