using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Core;

public class PatchManager
{
    public class PatchData
    {
        public static Random random = new Random();
        public static string GetRandName()
        {
            int p1, p2;
            p1 = 97; p2 = 118;
            string rdText = null;
            for(int i = 0; i < 24; i++)
            {
                rdText += (char)random.Next(p1, p2); 
            }
            return rdText;
        }
        public PatchData(int n, int m, string fileName)
        {
            projectID = n;
            fileID = m;
            this.fileName = fileName;
            tempFileID = GetRandName();
        }

        public PatchData(string downloadURL, string fileName)
        {
            this.downloadURL = downloadURL;
            this.fileName = fileName;

            projectID = -1;
            fileID = -1;
            tempFileID = GetRandName();
        }
        public string downloadURL;
        public string fileName;


        public int projectID;
        public int fileID;

        public string tempFileID;
    }


    public int downloadFileCount = 20;
    public int downloadCompleteCount = 0;
    public int downloadFailedCount = 0;
    public List<PatchData> patchDatas = new List<PatchData>();
   
    public delegate void DownloadSingleCompleteDelegate(int downloadFileCount, int downloadCompleteCount, int downloadFailedCount);
    public delegate void DownloadCompleteDelegate(int downloadFileCount, int downloadCompleteCount, int downloadFailedCount);

    public DownloadSingleCompleteDelegate downloadSingleCompleteDelegate;
    public DownloadCompleteDelegate       downloadCompleteDelegate;

    public string tempExtention = ".file";
    public static void DownloadComplete()
    {
       
    }

    public bool PatchCheck()
    {
        var mods = GetMods();
        var modsPackZip = GetDownloadeModsPackZip();
        foreach(var zip in modsPackZip)
        {
            mods.Add(zip);
        }
        int equalCount = 0;

        for(int i = 0; i< patchDatas.Count; i++)
        {
           for(int j = 0; j < mods.Count; j++)
           {
                Debug.WriteLine(patchDatas[i].fileName +","+ mods[j].Name);
                if (patchDatas[i].fileName == mods[j].Name)
                {
                    equalCount++;
                    break;
                }
           
           }
        }

        if (equalCount == patchDatas.Count)
        {
            Debug.WriteLine(equalCount + "," + patchDatas.Count);
            return true;
        }

        return false;
    }
    public async void DownloadFile(PatchData data, DownloadSingleCompleteDelegate callback)
    {
        try
        {
            WebClient wc = new WebClient();
            wc.DownloadFileAsync(new Uri(data.downloadURL), @"patchData\"+  data.tempFileID  + tempExtention);
            wc.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) => {
                downloadCompleteCount += 1;
                callback(downloadFileCount, downloadCompleteCount, downloadFailedCount);

                if(downloadCompleteCount + downloadFailedCount == downloadFileCount)
                {
                    OnPatchComplete();
                }
            };
            wc.Dispose();
            
        }
        catch(Exception e)
        {
            downloadFailedCount += 1;
            callback(downloadFileCount, downloadCompleteCount, downloadFailedCount);
            if (downloadCompleteCount + downloadFailedCount == downloadFileCount)
            {
                OnPatchComplete();
            }
        }
    }
    public async Task MakeDownloadURL(PatchData data)
    {
        if (data.downloadURL == null)
        {
            var link = await CurseApis.getDownloadURL(data.projectID, data.fileID);
            data.downloadURL = link;
        }
    }
    public async void PatchStart(DownloadSingleCompleteDelegate callback)
    {

        bool isInstalledJava = await JAVAInstaller.isJavaInstalled();
        if(isInstalledJava == false) 
        {
             await JAVAInstaller.DownloadJava();
        }

        downloadFileCount = patchDatas.Count;
        if (PatchCheck())
        {
            Debug.WriteLine(PatchCheck());
            downloadCompleteDelegate(downloadFileCount, downloadCompleteCount, downloadFailedCount);
            return;
        }
        else
        {
            foreach (var m in patchDatas)
            {
                await MakeDownloadURL(m);
            }
            System.Threading.Thread.Sleep(1000);
            foreach (var m in patchDatas)
            {
                DownloadFile(m, callback);
            }
        }
    }

    public void OnPatchComplete()
    {

        CreateMineCraftFolder();
        DirectoryInfo patchFolder = new DirectoryInfo("patchData");
        var files = patchFolder.GetFiles();
        Empty(new DirectoryInfo(MineCraftInfo.MinecraftFolderName + "/mods"));
        foreach (var m in files)
        {
            var data = patchDatas.Find(x => x.tempFileID + tempExtention == m.Name);
            var fileName = (data != null) ? data.fileName : m.Name;
            if (fileName.Contains(".jar"))
            {
                System.IO.File.Move(m.FullName, MineCraftInfo.MinecraftFolderName + "/mods/" + fileName);
            }
            else if (fileName.Contains(".zip"))
            {
                if (File.Exists(MineCraftInfo.MinecraftFolderName + "/" + fileName) == false)
                {
                    WpfApp3.DownloadWindowLayout1.instance.downloadtext.Text = "모드팩 압축 해제중입니다.";
                    WpfApp3.DownloadWindowLayout1.instance.downloadtext.UpdateLayout();

                    System.IO.File.Move(m.FullName, config.modpackDownload + "/" + fileName);
                    System.IO.FileInfo info = new FileInfo(config.modpackDownload + "/" + fileName);
                   
                    ExtractZipFile(info.FullName, null, config.modpackExtract);
                }
            }


        }


        downloadCompleteDelegate(downloadFileCount, downloadCompleteCount, downloadFailedCount);

    }

    public static void DecompressZip(FileInfo fileToDecompress)
    {

        using (Stream targetStream = new GZipInputStream(File.OpenRead(fileToDecompress.FullName)))
        {
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(targetStream, TarBuffer.DefaultBlockFactor))
            {
                    tarArchive.ExtractContents(@"C:\");
            }
        }
    }

    public void Empty(System.IO.DirectoryInfo directory)
    {
    
        foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
        foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
    }



    public void InitPatchList()
    {
        patchDatas.Add(new PatchData(32274, 2498312, "JouryMinimap.jar"));
        patchDatas.Add(new PatchData(32274, 2498312, "JouryMinimap2.jar"));
        patchDatas.Add(new PatchData(32274, 2498312, "JouryMinimap3.jar"));
        patchDatas.Add(new PatchData(32274, 2498312, "JouryMinimap4.jar"));
        patchDatas.Add(new PatchData(32274, 2498312, "JouryMinimap5.jar"));
        patchDatas.Add(new PatchData("https://minecraft.curseforge.com/projects/terrafirmapunk/files/2336047/download", "TerrafirmaPunk.zip"));
    }
    public void PatchMain()
    {
        InitPatchList();

    }

    public string GetLauncer(string folderPath)
    {
        return AppDomain.CurrentDomain.BaseDirectory + MineCraftInfo.MinecraftFolderName+"/"+folderPath;
    }

    public void CreateMineCraftFolder()
    {
        if(Directory.Exists(MineCraftInfo.MinecraftFolderName) == false)
        {
            Directory.CreateDirectory(MineCraftInfo.MinecraftFolderName);
        }
    }


    public void WritePatchList()
    {   

        var m = this.GetMods();

        string hashList = null;
        foreach (var n in m)
        {
            string value = "";
            using (var fs = File.OpenRead(n.FullName))
            using (var md5 = new MD5CryptoServiceProvider())
                value = string.Join("", md5.ComputeHash(fs).ToArray().Select(i => i.ToString("X2")));


            hashList += n.Name + "," + value + "\n";
        }
        File.WriteAllText("patchList2.txt", hashList);

    }

    public List<FileInfo> GetDownloadeModsPackZip()
    {
        System.IO.DirectoryInfo dirInfo = new DirectoryInfo("dummy");
        var modFiles = dirInfo.GetFiles();
        List<FileInfo> fileInfoList = new List<FileInfo>();
        for (int i = 0; i < modFiles.Length; i++)
        {
            if (modFiles[i].Extension == ".zip")
                fileInfoList.Add(modFiles[i]);
        }
        return fileInfoList;
    }
    //모드폴더내의 파일들을 가져온다.
    public List<FileInfo> GetMods()
    {
       System.IO.DirectoryInfo dirInfo = new DirectoryInfo(GetLauncer("mods"));
       var modFiles = dirInfo.GetFiles();
       List<FileInfo> fileInfoList = new List<FileInfo>();
       for(int i = 0; i < modFiles.Length; i++)
       {
            if(modFiles[i].Extension == ".jar")
            fileInfoList.Add(modFiles[i]);
       }
       return fileInfoList;
    }


    public static void ExtractZipFile(string archiveFilenameIn, string password, string outFolder, string justThisFile = null)
    {
        ICSharpCode.SharpZipLib.Zip.ZipFile zf = null;
        try
        {
            FileStream fs = File.OpenRead(archiveFilenameIn);
            zf = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs);
            if (!String.IsNullOrEmpty(password))
            {
                zf.Password = password;   
            }
            foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry in zf)
            {
                if (!zipEntry.IsFile)
                {
                    continue;          
                }
                if (!String.IsNullOrEmpty(justThisFile) && zipEntry.Name != justThisFile)
                {
                    continue;
                }
                String entryFileName = zipEntry.Name;
                byte[] buffer = new byte[4096];     // 4K is optimum
                Stream zipStream = zf.GetInputStream(zipEntry);

                // Manipulate the output filename here as desired.
                String fullZipToPath = Path.Combine(outFolder, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);

                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                // of the file, but does not waste memory.
                // The "using" will close the stream even if an exception occurs.
                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipStream, streamWriter, buffer);
                }
            }
        }
        finally
        {
            if (zf != null)
            {
                zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                zf.Close(); // Ensure we release resources
            }
        }
    }
}

