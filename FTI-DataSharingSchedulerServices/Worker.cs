namespace FTI_DataSharingSchedulerServices;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private string SchedulerConfigFolder
    {
        get;
        set;
    } = @"C:\ProgramData\FairbancData";

    public Int16 Date1
    {
        get; private set;
    }
    public Int16 Date2
    {
        get; private set;
    }
    public Int16 Date3
    {
        get; private set;
    }
    public string Time
    {
        get; private set;
    }
    public string Sales
    {
        get; private set;
    }
    public string Repayment
    {
        get; private set;
    }
    public string Outlet
    {
        get; private set;
    }
    public string DataFolder
    {
        get; private set;
    }
    public int RunHour
    {
        get; private set;
    } = -1;

    public int RunMinute
    {
        get; private set;
    } = 0;

    public string DTid
    {
        get; private set;
    } = "0";

    public string DistName
    {
        get; private set;
    } = "Test";
    public string AppWorkingFolder
    {
        get; private set;
    }  

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        try
        {
            AppWorkingFolder = SchedulerConfigFolder + @"\Datasharing-result";
        }
        catch (Exception)
        {
            AppWorkingFolder = @"C:\ProgramData\FairbancData";
            _logger.LogError("Cannot set application working folder (the default is: current user 'Download' folder) !");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            // ---- Start the code below, don't change the code BEFORE this line ---- //
            // ---- vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv ---- //
            // Dont forget to re-read the config file
            await InitializeAsync();
            // Then check if it's intended time to run

            bool v1 = (DateTime.Now.Hour == RunHour && DateTime.Now.Minute == RunMinute);
            bool v2 = (DateTime.Now.Day == Date1 || DateTime.Now.Day == Date2 || DateTime.Now.Day == Date3);
            _logger.LogInformation((v1 && v2).ToString());
            if ((DateTime.Now.Hour == RunHour && DateTime.Now.Minute == RunMinute) &&
               ( DateTime.Now.Day == Date1 || DateTime.Now.Day == Date2 || DateTime.Now.Day == Date3))
            {

                // Then perform the data sharing upload task
                await PerformTask();
            }

            // ---- Do add the code above, don't change the code AFTER this line ---- //
            await Task.Delay (TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            await ReadFileAsync(SchedulerConfigFolder + "\\DateTimeInfo.ini");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message); ;
        }

    }
    private async Task ReadFileAsync(string filePath)
    {
        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            for (var i = 0; i < lines.Length; i++)
            {
                switch (lines[i])
                {
                    case "[DATE#1]":
                        Date1 =Convert.ToInt16( lines[i + 1]);
                        break;
                    case "[DATE#2]":
                        Date2 = Convert.ToInt16(lines[i + 1]);
                        break;
                    case "[DATE#3]":
                        Date3 = Convert.ToInt16(lines[i + 1]);
                        break;
                    case "[TIME]":
                        Time = lines[i + 1];
                        RunHour = Convert.ToInt32(Time.Substring(0, 2)); RunMinute = Convert.ToInt32(Time.Substring(3, 2));
                        break;
                    case "[SALES]":
                        Sales = lines[i + 1];
                        break;
                    case "[REPAYMENT]":
                        Repayment = lines[i + 1];
                        break;
                    case "[OUTLET]":
                        Outlet = lines[i + 1];
                        break;
                    case "[FOLDER]":
                        DataFolder = lines[i + 1];
                        break;
                    case "[DTID]":
                        DTid = lines[i + 1];
                        break;
                    case "[DTNAME]":
                        DistName = lines[i + 1];
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    private async Task PerformTask()
    {
        try
        {
            _logger.LogInformation(">> At {time} performing data upload by executing Data Sharing app at the specified time.", DateTimeOffset.Now);

            _logger.LogInformation($">>>> [RESULT] File info value in sequence are {Date1} ,{Date2} ,{Date3} ,{Time} ,{Sales}, {Repayment}, {Outlet}, {DataFolder} {DTid} and {DistName} ...");
#if DEBUG
            var clsUploadProcess = new UploadProcess("Y", "Y", Sales, Repayment, Outlet, DataFolder, DTid, "Testing_Only", AppWorkingFolder, _logger);
#else
            var clsUploadProcess = new UploadProcess("Y","Y",Sales,Repayment, Outlet, DataFolder, DTid, DistName, AppWorkingFolder, _logger);
#endif
            // Execute Upload process class
            clsUploadProcess.Main();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Data Sharing app at {time}", DateTimeOffset.Now);
        }
    }
}
