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
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Tar;
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
        downloadFileCount = patchDatas.Count;
        foreach (var m in patchDatas)
        {
            await MakeDownloadURL(m);
            Debug.WriteLine("Make Downlad URI");
        }
        System.Threading.Thread.Sleep(1000);
        foreach (var m in patchDatas)
        {
            DownloadFile(m, callback);
            Debug.WriteLine("Patch!");
        }
        
    }

    public void OnPatchComplete()
    {
        
        downloadCompleteDelegate(downloadFileCount, downloadCompleteCount, downloadFailedCount);
        CreateMineCraftFolder();
        DirectoryInfo patchFolder = new DirectoryInfo("patchData");
        var files = patchFolder.GetFiles();
        Empty(new DirectoryInfo(".minecraft/mods"));
        foreach (var m in files)
        {
            var data = patchDatas.Find(x => x.tempFileID + tempExtention == m.Name);
            var fileName = (data != null) ? data.fileName : m.Name;
            if (fileName.Contains(".jar"))
            {
                System.IO.File.Move(m.FullName, ".minecraft/mods/" + fileName);
            }
            else if (fileName.Contains(".zip")) 
            {
                System.IO.File.Move(m.FullName, ".minecraft/" + fileName);
                System.IO.FileInfo info = new FileInfo(".minecraft/" + fileName);
                Decompress(info);

            }
     

        }
    }

    public static void Decompress(FileInfo fileToDecompress)
    {
        using (Stream targetStream = new ZipInputStream(File.OpenRead(fileToDecompress.FullName)))
        {
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(targetStream, TarBuffer.DefaultBlockFactor))
            {
   
                    tarArchive.ExtractContents(@"c:\");
               
               
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
        return AppDomain.CurrentDomain.BaseDirectory + @".minecraft\"+folderPath;
    }

    public void CreateMineCraftFolder()
    {
        if(Directory.Exists(".minecraft") == false)
        {
            Directory.CreateDirectory(".minecraft");
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
}

