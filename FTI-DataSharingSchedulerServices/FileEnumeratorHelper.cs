using System;
using System.Collections.Generic;
using System.IO;

namespace FTI_DataSharingSchedulerServices;
public  struct FileEnumeratorHelper
{
    public static ILogger<Worker>? _logger;

    public enum PayOrSalesOrOutlet
    {
        PayPattern,
        SalesPattern,
        OutletPattern
    }

    private static string GetFiles(string strPattern, string DirPath)
    {
        var text = "*" + strPattern + "*.xls*";
        IEnumerable<FileInfo> enumerable = (from f in new DirectoryInfo(DirPath).GetFiles(text, SearchOption.TopDirectoryOnly)
                                            orderby f.LastWriteTime descending
                                            select f).Take(1);
        if (enumerable.Any())
        {
            return enumerable.First().FullName;
        }
        return ">>>> [OUTPUT] Tak ada file excel nama '*" + strPattern + "*'";
    }

    private static FileInfo? GetListFilesInfo(string strPattern, string DirPath)
    {
        var text = "*" + strPattern + "*.xls*";
        IEnumerable<FileInfo> enumerable = (from f in new DirectoryInfo(DirPath).GetFiles(text, SearchOption.TopDirectoryOnly)
                                            orderby f.LastWriteTime descending
                                            select f).Take(1);
        if (enumerable.Any())
        {
            return enumerable.First();
        }
        return null;
    }

    private static List<string>? strLatesFileOf(List<FileInfo> files)
    {
        files.RemoveAll((FileInfo item) => item == null);
        var text = "";
        var text2 = "";
        if (files.Any())
        {
            text = files.First().FullName;
            DateTime dateTime = files.First().LastWriteTime;
            foreach (FileInfo file in files)
            {
                DateTime lastWriteTime = file.LastWriteTime;
                if (lastWriteTime > dateTime)
                {
                    dateTime = lastWriteTime;
                    text = file.FullName;
                    text2 = file.Name;
                }
            }
            return new List<string> { text, text2 };
        }
        return null;
    }

    public static string GetLatesFileName(List<string> strFilePattern, string strPath, PayOrSalesOrOutlet mode)
    {
        List<string>? list2 = null;
        if (mode == PayOrSalesOrOutlet.SalesPattern)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (string item in strFilePattern)
            {
                _logger.LogInformation(">>>> [OUTPUT] Mencari file Penjualan yang mempunyai nama '*" + item.Trim() + "*'...");
                _logger.LogInformation(GetFiles(item, strPath) + " \n");
                if (GetListFilesInfo(item, strPath) != null)
                {
                    list.Add(GetListFilesInfo(item, strPath));
                }
            }
            list2 = strLatesFileOf(list);
            if (list2 != null)
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] File Penjualan yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] Tidak ada file Penjualan di temukan !! ");
                _logger.LogInformation("********************************************\n");
            }
        }
        else if (mode == PayOrSalesOrOutlet.PayPattern)
        {
            List<FileInfo> list3 = new List<FileInfo>();
            foreach (var item2 in strFilePattern)
            {
                _logger.LogInformation(">>>> [OUTPUT] Mencari file Pembayaran untuk yang mempunyai nama '*" + item2.Trim() + "*'...");
                _logger.LogInformation(GetFiles(item2, strPath) + " \n");
                list3.Add(GetListFilesInfo(item2, strPath));
            }
            list2 = strLatesFileOf(list3);
            if (list2 != null)
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] File Pembayaran yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] Tidak ada file Pembayaran di temukan !!");
                _logger.LogInformation("********************************************\n");
            }
        }
        else
        {
            List<FileInfo> list4 = new List<FileInfo>();
            foreach (var item3 in strFilePattern)
            {
                _logger.LogInformation(">>>> [OUTPUT] Mencari file Outlet untuk yang mempunyai nama '*" + item3.Trim() + "*'...");
                _logger.LogInformation(GetFiles(item3, strPath) + " \n");
                list4.Add(GetListFilesInfo(item3, strPath));
            }
            list2 = strLatesFileOf(list4);
            if (list2 != null)
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] File Outlet yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] Tidak ada file Outlet di temukan !!");
                _logger.LogInformation("********************************************\n");
            }
        }

        if (list2 != null)
        {
            _logger.LogInformation(">>>> [OUTPUT] Dan Waktu akses terakhir file tsb : " + File.GetLastWriteTime(list2.First()).ToLocalTime());
            _logger.LogInformation("********************************************\n");
            return list2.First();
        }

        return "";
    }

    public static string GetLatesFileName(List<string> strFilePattern, string strPath, PayOrSalesOrOutlet mode, string strSearchSubFolder)
    {
        List<string>? list2 = null;
        if (mode == PayOrSalesOrOutlet.SalesPattern)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (string item in strFilePattern)
            {
                _logger.LogInformation(">>>> [OUTPUT] Mencari file Penjualan yang mempunyai nama '*" + item.Trim() + "*'...");
                _logger.LogInformation(GetFiles(item, strPath));
                if (GetListFilesInfo(item, strPath) != null)
                {
                    list.Add(GetListFilesInfo(item, strPath));
                }
                if (!(strSearchSubFolder == "Y"))
                {
                    break;
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(strPath);
                DirectoryInfo[] directories = directoryInfo.GetDirectories();
                foreach (DirectoryInfo directoryInfo2 in directories)
                {
                    if (GetListFilesInfo(item, directoryInfo2.FullName) != null && directoryInfo2.Name != "upload")
                    {
                        _logger.LogInformation(GetFiles(item, directoryInfo2.FullName));
                        list.Add(GetListFilesInfo(item, directoryInfo2.FullName));
                    }
                }
            }
            list2 = strLatesFileOf(list);
            if (list2 != null)
            {
                _logger.LogInformation("============================================");
                _logger.LogInformation(">>>> [RESULT] File Penjualan yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] Tidak ada file Penjualan di temukan !! ");
                _logger.LogInformation("********************************************\n");
            }
        }
        else if (mode == PayOrSalesOrOutlet.PayPattern)
        {
            List<FileInfo> list3 = new List<FileInfo>();
            foreach (var item2 in strFilePattern)
            {
                _logger.LogInformation(">>>> [OUTPUT] Mencari file Pembayaran yang mempunya nama '*" + item2.Trim() + "*'...");
                _logger.LogInformation(GetFiles(item2, strPath));
                if (GetListFilesInfo(item2, strPath) != null)
                {
                    list3.Add(GetListFilesInfo(item2, strPath));
                }
                if (!(strSearchSubFolder == "Y"))
                {
                    break;
                }
                DirectoryInfo directoryInfo3 = new DirectoryInfo(strPath);
                DirectoryInfo[] directories2 = directoryInfo3.GetDirectories();
                foreach (DirectoryInfo directoryInfo4 in directories2)
                {
                    if (GetListFilesInfo(item2, directoryInfo4.FullName) != null && directoryInfo4.Name != "upload")
                    {
                        _logger.LogInformation(GetFiles(item2, directoryInfo4.FullName));
                        list3.Add(GetListFilesInfo(item2, directoryInfo4.FullName));
                    }
                }
            }
            list2 = strLatesFileOf(list3);
            if (list2 != null)
            {
                _logger.LogInformation("============================================");
                _logger.LogInformation(">>>> [RESULT] File Pembayaran yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] Tidak ada file Pembayaran di temukan !!");
                _logger.LogInformation("********************************************\n");
            }
        }
        else
        {
            List<FileInfo> list4 = new List<FileInfo>();
            foreach (var item3 in strFilePattern)
            {
                _logger.LogInformation(">>>> [OUTPUT] Mencari file Outlet yang mempunya nama '*" + item3.Trim() + "*'...");
                _logger.LogInformation(GetFiles(item3, strPath));
                if (GetListFilesInfo(item3, strPath) != null)
                {
                    list4.Add(GetListFilesInfo(item3, strPath));
                }
                if (!(strSearchSubFolder == "Y"))
                {
                    break;
                }
                DirectoryInfo directoryInfo5 = new DirectoryInfo(strPath);
                DirectoryInfo[] directories3 = directoryInfo5.GetDirectories();
                foreach (DirectoryInfo directoryInfo6 in directories3)
                {
                    if (GetListFilesInfo(item3, directoryInfo6.FullName) != null && directoryInfo6.Name != "upload")
                    {
                        _logger.LogInformation(GetFiles(item3, directoryInfo6.FullName));
                        list4.Add(GetListFilesInfo(item3, directoryInfo6.FullName));
                    }
                }
            }
            list2 = strLatesFileOf(list4);
            if (list2 != null)
            {
                _logger.LogInformation("============================================");
                _logger.LogInformation(">>>> [RESULT] File Outlet yang akan di upload adalah : \n" + list2.First());
            }
            else
            {
                _logger.LogInformation("********************************************");
                _logger.LogInformation(">>>> [RESULT] Tidak ada file Outlet di temukan !!");
                _logger.LogInformation("********************************************\n");
            }
        }
        if (list2 != null)
        {
            _logger.LogInformation(">>>> [OUTPUT] Dan waktu akses terakhir file tsb : " + File.GetLastWriteTime(list2.First()).ToLocalTime());
            _logger.LogInformation("============================================\n");
            return list2.First();
        }
        else
        {
            return "";
        }
    }

    public static void Finnished()
    {
        _logger.LogInformation(">>>> [OUTPUT] Proses Data Sharing Excel akan segera selesai !");
        _logger.LogInformation(">>>> [OUTPUT] Mohon di tunggu, data sedang di upload.\n");

    }
}
