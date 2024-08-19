using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace FTI_DataSharingSchedulerServices;

public static class FileEnumeratorHelper
{
    // Use a private setter to allow initialization but prevent external modification
    public static ILogger<Worker>? Logger { get; private set; }

    public enum FileType
    {
        Sales,
        Payment,
        Outlet
    }

    // Initialize the logger
    public static void SetLogger(ILogger<Worker> logger)
    {
        Logger = logger;
    }

    private static string GetFiles(string strPattern, string dirPath)
    {
        var searchPattern = $"*{strPattern}*.xls*";
        var file = new DirectoryInfo(dirPath)
            .GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        return file != null 
            ? file.FullName 
            : $">>>> [OUTPUT] No excel file found with name '*{strPattern}*'";
    }

    private static FileInfo? GetListFilesInfo(string strPattern, string dirPath)
    {
        var searchPattern = $"*{strPattern}*.xls*";
        return new DirectoryInfo(dirPath)
            .GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();
    }

    private static List<string>? GetLatestFileInfo(List<FileInfo> files)
    {
        files.RemoveAll(item => item == null);
        if (!files.Any()) return null;

        var latestFile = files.OrderByDescending(f => f.LastWriteTime).First();
        return new List<string> { latestFile.FullName, latestFile.Name };
    }

    public static string GetLatestFileName(string directory, FileType fileType)
    {
        List<FileInfo> fileList = new List<FileInfo>();
        string fileTypeName = fileType.ToString();

        foreach (string pattern in new[] { fileTypeName })
        {
            Logger?.LogInformation($">>>> [OUTPUT] Searching for {fileTypeName} file with name '*{pattern.Trim()}*'...");
            Logger?.LogInformation(GetFiles(pattern, directory) + " \n");
            var fileInfo = GetListFilesInfo(pattern, directory);
            if (fileInfo != null)
            {
                fileList.Add(fileInfo);
            }
        }

        var latestFile = GetLatestFileInfo(fileList);
        if (latestFile != null)
        {
            Logger?.LogInformation("********************************************");
            Logger?.LogInformation($">>>> [RESULT] {fileTypeName} file to be uploaded is: \n{latestFile[0]}");
            Logger?.LogInformation($">>>> [OUTPUT] Last access time of the file: {File.GetLastWriteTime(latestFile[0]).ToLocalTime()}");
            Logger?.LogInformation("********************************************\n");
            return latestFile[0];
        }
        else
        {
            Logger?.LogInformation("********************************************");
            Logger?.LogInformation($">>>> [RESULT] No {fileTypeName} file found!");
            Logger?.LogInformation("********************************************\n");
            return string.Empty;
        }
    }

    public static void Finished(string sourceDir, string destDir)
    {
        Logger?.LogInformation(">>>> [OUTPUT] Excel Data Sharing process will be completed soon!");
        Logger?.LogInformation(">>>> [OUTPUT] Please wait, data is being uploaded.\n");
    }


}