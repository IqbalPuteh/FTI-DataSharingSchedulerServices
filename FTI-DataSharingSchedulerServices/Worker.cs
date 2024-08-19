namespace FTI_DataSharingSchedulerServices;

public class Worker : BackgroundService
{
    private const string DEFAULT_FOLDER = @"C:\ProgramData\FairbancData";

    private readonly ILogger<Worker> _logger;
    private readonly string _schedulerConfigFolder = DEFAULT_FOLDER;

    public Int16 Date1 { get; private set; }
    public Int16 Date2 { get; private set; }
    public Int16 Date3 { get; private set; }
    public string Time { get; private set; }
    public string Sales { get; private set; }
    public string Repayment { get; private set; }
    public string Outlet { get; private set; }
    public string DataFolder { get; private set; }
    public int RunHour { get; private set; } = -1;
    public int RunMinute { get; private set; } = 0;
    public string DTid { get; private set; } = "0";
    public string DistName { get; private set; } = "Test";
    public string AppWorkingFolder { get; private set; }

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        try
        {
            AppWorkingFolder = _schedulerConfigFolder + @"\Datasharing-result";
        }
        catch (Exception)
        {
            AppWorkingFolder = DEFAULT_FOLDER;
            _logger.LogError("Cannot set application working folder (the default is: current user 'Download' folder) !");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            await InitializeAsync();

            bool isScheduledTime = DateTime.Now.Hour == RunHour && DateTime.Now.Minute == RunMinute;
            bool isScheduledDay = new[] { Date1, Date2, Date3 }.Contains((short)DateTime.Now.Day);

            if (isScheduledTime && isScheduledDay)
            {
                await PerformTask();
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            await ReadFileAsync(_schedulerConfigFolder + "\\DateTimeInfo.ini");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
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
                    case "[DATE#1]": Date1 = Convert.ToInt16(lines[i + 1]); break;
                    case "[DATE#2]": Date2 = Convert.ToInt16(lines[i + 1]); break;
                    case "[DATE#3]": Date3 = Convert.ToInt16(lines[i + 1]); break;
                    case "[TIME]":
                        Time = lines[i + 1];
                        (RunHour, RunMinute) = (Convert.ToInt32(Time[..2]), Convert.ToInt32(Time.Substring(3, 2)));
                        break;
                    case "[SALES]": Sales = lines[i + 1]; break;
                    case "[REPAYMENT]": Repayment = lines[i + 1]; break;
                    case "[OUTLET]": Outlet = lines[i + 1]; break;
                    case "[FOLDER]": DataFolder = lines[i + 1]; break;
                    case "[DTID]": DTid = lines[i + 1]; break;
                    case "[DTNAME]": DistName = lines[i + 1]; break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
        }
    }

    private async Task PerformTask()
    {
        try
        {
            _logger.LogInformation(">> At {time} performing data upload by executing Data Sharing app at the specified time.", DateTimeOffset.Now);

            _logger.LogInformation($">>>> [RESULT] File info value in sequence are {Date1} ,{Date2} ,{Date3} ,{Time} ,{Sales}, {Repayment}, {Outlet}, {DataFolder} {DTid} and {DistName} ...");
#if DEBUG
            var uploadProcess = new UploadProcess("Y", "Y", Sales, Repayment, Outlet, DataFolder, DTid, "Testing_Only", AppWorkingFolder, _logger);
#else
            var uploadProcess = new UploadProcess("Y","Y",Sales,Repayment, Outlet, DataFolder, DTid, DistName, AppWorkingFolder, _logger);
#endif
            await uploadProcess.ExecuteAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Data Sharing app at {time}", DateTimeOffset.Now);
        }
    }
}