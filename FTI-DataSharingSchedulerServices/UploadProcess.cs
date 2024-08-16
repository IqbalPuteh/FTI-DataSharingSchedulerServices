using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace FTI_DataSharingSchedulerServices;
public class UploadProcess
{
    private readonly ILogger<Worker> _logger;

    private static string strStatusCode = "-1";

    private static string strResponseBody = "";

    private static string strZipFile = "";

    private static string strlogFileName = "";

    private static string strSandboxBoolean = "";

    private static string strSecureHTTP = "Y";

    private static string strSalesPattern = "";

    private static string strPayPattern = "";

    private static string strOutletPattern = "";

    private static string strDistID = "";

    private static string strDistName = "";

    private static string strDsDataSourceDir = "";

    private static string strDsExpDir = "";

    private static string strDsUploadDir = "";

    private static string strDsWorkingDir = "";

    private static string strSearchSubFolder = "N";

    private static void CheckandRefreshFolder(string location)
    {
        try
        {
            if (Directory.Exists(location))
            {
                Directory.Delete(location, true);
            }
            Directory.CreateDirectory(location);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static bool IsDirectoryEmpty(string strPath)
    {
        string[] files = Directory.GetFiles(strPath);
        if (files.Length == 0)
        {
            return true;
        }
        return false;
    }

    public static void WriteLog(string logMessage, string strFileName)
    {
        using StreamWriter streamWriter = File.AppendText(strFileName);
        streamWriter.Write("\r\nLog Entry : ");
        streamWriter.Write(DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString());
        streamWriter.WriteLine(" - :" + logMessage);
    }

    private static string SendReq(string strFileDataInfo, string strSandboxBool, string strSecureHTTP)
    {
        try
        {
            string text = "";
            string text2 = "";
            if (strSandboxBool == "Y")
            {
                text2 = "KQtbMk32csiJvm8XDAx2KnRAdbtP3YVAnJpF8R5cb2bcBr8boT3dTvGc23c6fqk2NknbxpdarsdF3M4V";
                text = ((!(strSecureHTTP == "Y")) ? "http://sandbox.fairbanc.app/api/documents" : "https://sandbox.fairbanc.app/api/documents");
            }
            else
            {
                text2 = "2S0VtpYzETxDrL6WClmxXXnOcCkNbR5nUCCLak6EHmbPbSSsJiTFTPNZrXKk2S0VtpYzETxDrL6WClmx";
                text = ((!(strSecureHTTP == "Y")) ? "http://dashboard.fairbanc.app/api/documents" : "https://dashboard.fairbanc.app/api/documents");
            }
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, text);
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            multipartFormDataContent.Add(new StringContent(text2), "api_token");
            multipartFormDataContent.Add(new ByteArrayContent(File.ReadAllBytes(strFileDataInfo)), "file", Path.GetFileName(strFileDataInfo));
            httpRequestMessage.Content = multipartFormDataContent;
            HttpResponseMessage httpResponseMessage = httpClient.Send(httpRequestMessage);
            Thread.Sleep(5000);
            httpResponseMessage.EnsureSuccessStatusCode();
            strResponseBody = httpResponseMessage.ToString();
            string[] array = strResponseBody.Split(':', ',');
            return array[1].Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return "-1";
        }
    }

    private static bool AreExcelFilesPresent(string folderPath)
    {
        string[] excelExtensions = { ".xls", ".xlsx", ".xlsm" };
        return Directory.GetFiles(folderPath)
            .Any(file => excelExtensions.Contains(Path.GetExtension(file).ToLower()));
    }

    private static void ShowNoExcelFilesPopup(string folderPath)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "DS-PopUp.exe",
            LoadUserProfile = true,
            Arguments = folderPath
        };
        var process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }

    public void Main()
    {
        try
        {
            var strCurrDate = DateTime.Now.ToString("yyyy") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd");
            var strDsPeriod = DateTime.Now.ToString("yyyy") + DateTime.Now.AddMonths(-1).ToString("MM");
            DateTime dateTime = DateTime.Now.AddMonths(-1);

            strDsPeriod = dateTime.ToString("yyyy") + dateTime.ToString("MM");
            var intNoOfDays = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

            // ReadConfigSetting(); => its already done in the constructor

            strlogFileName = "DEBUG-" + strDistID + "-" + strDistName + "-" + strDsPeriod + ".log";

            strDsExpDir += strDsPeriod;
            CheckandRefreshFolder(strDsExpDir);
            CheckandRefreshFolder(strDsUploadDir);

            strlogFileName = strDsWorkingDir + Path.DirectorySeparatorChar + strlogFileName;
            //WriteLog("Start Process Excel File sales and payment", strlogFileName);
            WriteLog("Starting proces of Excel file sales, payment and outlet.", strlogFileName);
            WriteLog("Uploaded via FTI Submission App - Window Service.", strlogFileName);
            WriteLog($"Using Working folder -> {strDsWorkingDir} , Zip folder -> {strDsExpDir} , Upload Folder -> {strDsUploadDir}", strlogFileName);

            var strSalesFileName = "";
            var strPayFileName = "";
            var strOutletFileName = "";

            _logger.LogInformation(">>>> [OUTPUT] Memulai applikasi...\n");
            if (strSalesPattern != "")
            {
                List<string> strFilePattern = (from s in strSalesPattern.Split(new char[1] { ',' })
                                               select (s)).ToList();
                strSalesFileName = FileEnumeratorHelper.GetLatesFileName(strFilePattern, strDsDataSourceDir, FileEnumeratorHelper.PayOrSalesOrOutlet.SalesPattern, strSearchSubFolder);
                if (!(strSalesFileName != ""))
                {
                    WriteLog("No Sales data will be processed.", strlogFileName);
                }
            }
            if (strPayPattern != "")
            {
                List<string> strFilePattern2 = (from s in strPayPattern.Split(new char[1] { ',' })
                                                select (s)).ToList();
                strPayFileName = FileEnumeratorHelper.GetLatesFileName(strFilePattern2, strDsDataSourceDir, FileEnumeratorHelper.PayOrSalesOrOutlet.PayPattern, strSearchSubFolder);
                if (!(strPayFileName != ""))
                {
                    WriteLog("No Payment data will be processed.", strlogFileName);
                }
            }
            if (strOutletPattern != "")
            {
                List<string> strFilePattern3 = (from s in strOutletPattern.Split(new char[1] { ',' })
                                                select (s)).ToList();
                strOutletFileName = FileEnumeratorHelper.GetLatesFileName(strFilePattern3, strDsDataSourceDir, FileEnumeratorHelper.PayOrSalesOrOutlet.OutletPattern, strSearchSubFolder);
                if (!(strOutletFileName != ""))
                {
                    WriteLog("No Outlet data will be processed.", strlogFileName);
                }
            }

            //WriteLog("File to be copied are: " + strSalesFileName + " and " + strPayFileName, strlogFileName);
            WriteLog("File Penjualan yang di proses adalah: " + strSalesFileName, strlogFileName);
            WriteLog("File Pembayaran yang di proses adalah: " + " , " + strPayFileName, strlogFileName);
            WriteLog("File Kustomer yang di proses adalah: " + strOutletFileName, strlogFileName);

            if (strSalesFileName.Trim() != "" || strSalesPattern.Trim() != "")
            {
                var strFileDataName = ((!strSalesFileName.ToLower().EndsWith("xls")) ? ("ds-" + strDistID + "-" + strDistName + "-" + strDsPeriod + "_SALES.xlsx") : ("ds-" + strDistID + "-" + strDistName + "-" + strDsPeriod + "_SALES.xls"));
                if (strSalesFileName.Trim() != "")
                {
                    try
                    {
                        File.Copy(strSalesFileName, strDsUploadDir + Path.DirectorySeparatorChar + strFileDataName, true);
                        File.Delete(strSalesFileName);
                    }
                    catch (Exception ex)
                    {
                        WriteLog("WARNING: Error occurred: " + ex.Message, strlogFileName);
                    }
                }
            }
            if (strPayFileName.Trim() != "" || strPayPattern.Trim() != "")
            {
                var strFileDataName = ((!strPayFileName.ToLower().EndsWith("xls")) ? ("ds-" + strDistID + "-" + strDistName + "-" + strDsPeriod + "_PAYMENT.xlsx") : ("ds-" + strDistID + "-" + strDistName + "-" + strDsPeriod + "_PAYMENT.xls"));
                if (strPayFileName.Trim() != "")
                {
                    try
                    {
                        File.Copy(strPayFileName, strDsUploadDir + Path.DirectorySeparatorChar + strFileDataName, true);
                        File.Delete(strPayFileName);
                    }
                    catch (Exception ex2)
                    {
                        WriteLog("WARNING: Error occurred: " + ex2.Message, strlogFileName);
                    }
                }
            }

            /* penambahan logic utuk upload file master outlet */
            if (strOutletPattern.Trim() != "" || strOutletFileName.Trim() != "")
            {
                var strFileDataName = ((!strOutletFileName.ToLower().EndsWith("xls")) ? ("ds-" + strDistID + "-" + strDistName + "-" + strDsPeriod + "_OUTLET.xlsx") : ("ds-" + strDistID + "-" + strDistName + "-" + strDsPeriod + "_OUTLET.xls"));
                if (strOutletFileName.Trim() != "")
                {
                    try
                    {
                        File.Copy(strOutletFileName, strDsUploadDir + Path.DirectorySeparatorChar + strFileDataName, true);
                        File.Delete(strOutletFileName);
                    }
                    catch (Exception ex3)
                    {
                        WriteLog("WARNING: Error occurred: " + ex3.Message, strlogFileName);
                    }
                }
            }
            /* selesai menambah kan logic upload master outlet */

            if (!IsDirectoryEmpty(strDsUploadDir))
            {
                WriteLog("Copy process for Excel files (sales, payment,outlet) done, Start archive process.", strlogFileName);
                strZipFile = strDistID + "-" + strDistName + "_" + strDsPeriod + ".zip";
                ZipFile.CreateFromDirectory(strDsUploadDir, strDsExpDir + Path.DirectorySeparatorChar + strZipFile);
                //WriteLog("Archive process Excel file sales and payment done", strlogFileName);
                WriteLog("Archive process Excel file sales, payment and outlet done", strlogFileName);
                strStatusCode = SendReq(strDsExpDir + Path.DirectorySeparatorChar + strZipFile, strSandboxBoolean, strSecureHTTP);
                //WriteLog("Upload process Excel file sales and payment done", strlogFileName);
                WriteLog("Upload process Excel file sales, payment and outlet done", strlogFileName);
                if (strStatusCode == "200")
                {
                    WriteLog("Data Sharing - SELESAI", strlogFileName);
                }
                else
                {
                    WriteLog("WARNING:Gagal upload, Data Sharing cUrl STATUS CODE :" + strStatusCode, strlogFileName);
                }
            }
            else
            {
                WriteLog("WARNING: No uploaded file(s) found - Neither Sales and payment Excel Files Processed", strlogFileName);
            }
            SendReq(strlogFileName, strSandboxBoolean, strSecureHTTP);
             FileEnumeratorHelper.Finnished();
        }
        catch (Exception ex3)
        {
            if (strlogFileName.Trim() == "")
            {
                if (Directory.Exists(strDsWorkingDir)) 
                {
                    strlogFileName = strDsWorkingDir + Path.DirectorySeparatorChar + "ExcelDataSharingApp.log";
                }
            }
            WriteLog("WARNING: Error occurred: " + ex3.Message, strlogFileName);
        }

        //if (!AreExcelFilesPresent(strDsDataSourceDir))
        //{
        //    ShowNoExcelFilesPopup(strDsDataSourceDir);
        //}
    }

    public UploadProcess(string _strSandboxBoolean, string _strSecureHTTP, string _strSalesPattern, string _strPayPattern, string _strOutletPattern, string _strDataFolder, string _strDistID, string _strDistName, string _strWorkingFolder, ILogger<Worker> logger)
    {
        try
        {
            strSandboxBoolean = _strSandboxBoolean;
            strSecureHTTP = _strSecureHTTP;
            strSalesPattern = _strSalesPattern;
            strPayPattern = _strPayPattern;
            strOutletPattern = _strOutletPattern;
            strDistID = _strDistID;
            strDistName = _strDistName;
            strDsDataSourceDir = _strDataFolder;
            strDsWorkingDir = _strWorkingFolder;
            strDsExpDir = _strWorkingFolder + Path.DirectorySeparatorChar + "FTI-sharing";
            strDsUploadDir = _strWorkingFolder + Path.DirectorySeparatorChar + "FTI-upload";
            strSearchSubFolder = "N";
            _logger = logger;
            FileEnumeratorHelper._logger = logger;
        }
        catch (Exception Ex)
        {
            _logger.LogError("Unable to setup upload class configuration.", Ex);
        }

    }
}