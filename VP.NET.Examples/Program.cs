using VP.NET;
#pragma warning disable CS8321

/* To compress or decompress you only need to
 * vp.EnableCompression(); or vp.DisableCompression();
 * And the changes will be done using vp.SaveAsAsync("");
 * a vp version 3 indicate compression on and 2 compression off
 * All files will be always be decompressed if they are saved as compressed in the vp
*/

//Examples:
//DisplayVPData();
//await DisplayVPDataAsync();
//await CreateVPFromFolder();
//await CreateCompressedVPFromFolder();
//await ExtractEntireVP();
//await AddFolderToVPRoot();
//await AddFolderToVPFolder();
//await AddFileToVPFolder();
//await CreateVPFolderAndAdd();
//await DeleteFolderOrFile();
//await CompressStandaloneFile();
//await DecompressStandaloneFile();
//await CompressAExistingVP();
//await DecompressAExistingVP();
await CompressAllVpsInAFolder();

async Task CompressAllVpsInAFolder()
{
    string[] files = Directory.GetFiles("D:\\fs2_open\\FS2\\MVPS-4.6.8", "*.vp");
    await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (file, token) =>
    {
        Console.WriteLine("Compressing: " + file);
        var vp = new VPContainer(file);
        vp.EnableCompression();
        await vp.SaveAsAsync(file + "c");
    });
}

async Task DecompressAExistingVP()
{
    var vp = new VPContainer("D:\\mv_assets.vpc");
    vp.DisableCompression();
    await vp.SaveAsAsync("D:\\mv_assets2.vp");
}

async Task CompressAExistingVP()
{
    var vp = new VPContainer("D:\\mv_assets.vp");
    vp.EnableCompression();
    await vp.SaveAsAsync("D:\\mv_assets.vpc");
}

async Task DecompressStandaloneFile()
{
    var file_in = new FileStream("D:\\repo.json.lz4", FileMode.Open, FileAccess.Read, FileShare.Read);
    var file_out = new FileStream("D:\\repo2.json", FileMode.Create, FileAccess.Write, FileShare.None);

    await Task.Run(() => {
        var unCompressedSize = VPCompression.DecompressStream(file_in, file_out);
        Console.WriteLine("Decompressing Finished! Uncompressed Size: " + unCompressedSize + " Compressed Size: " + file_in.Length);
        file_in.Dispose();
        file_out.Dispose();
    });
}

async Task CompressStandaloneFile()
{
    var file_in = new FileStream("D:\\repo.json", FileMode.Open, FileAccess.Read,FileShare.Read);
    var file_out = new FileStream("D:\\repo.json.lz4", FileMode.Create, FileAccess.Write, FileShare.None);

    await Task.Run(() => { 
        var compressedSize = VPCompression.CompressStream(file_in, file_out);
        Console.WriteLine("Compressing Finished! Compressed Size: "+compressedSize + " Uncompressed Size: " + file_in.Length);
        file_in.Dispose();
        file_out.Dispose();
    });
}


async Task DeleteFolderOrFile()
{
    var vp = new VPContainer("D:\\mv_assets.vp");
    foreach (var file in vp.vpFiles)
    {
        if (file.type == VPFileType.Directory && file.info.name == "data")
        {
            foreach (var fileInDataFolder in file.files!)
            {
                if (fileInDataFolder.type == VPFileType.Directory && fileInDataFolder.info.name == "effects")
                {
                    //Delete "effects" folder and everything inside of it, changes will be aplied during saving. It is the same for individual files.
                    fileInDataFolder.Delete();
                }
            }
        }
    }

    await vp.SaveAsAsync("D:\\testDeleteFolder.vp");
}

async Task CreateVPFolderAndAdd()
{
    var vp = new VPContainer("D:\\root_fs2.vp");
    foreach (var file in vp.vpFiles)
    {
        if (file.type == VPFileType.Directory && file.info.name == "data")
        {
            //Create "effects" folder
            var effectsDir = file.CreateEmptyDirectory("effects");
            effectsDir.AddFile(new FileInfo("D:\\extract\\data\\effects\\Blue_Glow1_Small.dds"));
        }
    }

    await vp.SaveAsAsync("D:\\testNewFolder.vp");
}

async Task AddFileToVPFolder()
{
    var vp = new VPContainer("D:\\root_fs2.vp");
    foreach (var file in vp.vpFiles)
    {
        if (file.type == VPFileType.Directory && file.info.name == "data")
        {
            foreach (var fileInDataFolder in file.files!)
            {
                if (fileInDataFolder.type == VPFileType.Directory && fileInDataFolder.info.name == "tables")
                {
                    //Add single file to "tables" folder in vp, if it already exists it will be overriden
                    fileInDataFolder.AddFile(new FileInfo("D:\\extract\\data\\effects\\Blue_Glow1_Small.dds"));
                }
            }
        }
    }

    await vp.SaveAsAsync("D:\\testAddFolder.vp");
}

async Task AddFolderToVPFolder()
{
    var vp = new VPContainer("D:\\root_fs2.vp");
    foreach(var file in vp.vpFiles)
    {
        if(file.type == VPFileType.Directory && file.info.name == "data")
        {
            foreach(var fileInDataFolder in file.files! )
            {
                if(fileInDataFolder.type == VPFileType.Directory && fileInDataFolder.info.name == "tables")
                {
                    //This adds the content of D:\\extract\\data\\effects to data/tables/
                    //already existing files with the same path/name will be overriden
                    fileInDataFolder.AddDirectoryRecursive("D:\\extract\\data\\effects");
                }
            }
        }
    }

    await vp.SaveAsAsync("D:\\testAddFolder.vp");
}

async Task AddFolderToVPRoot()
{
    var vp = new VPContainer("D:\\root_fs2.vp");
    vp.AddFolderToRoot("D:\\extract");
    //The result is whatever was on the VP + what was on the "extract" folder, already existing files with the same path/name will be overriden
    await vp.SaveAsAsync("D:\\testAdd.vp");
    //You can enable compression to produce a compressed vp, if compression was already enabled this is not needed
    //vp.EnableCompression();
    //await vp.SaveAsAsync("D:\\testAdd.vpc");
}


async Task ExtractEntireVP()
{
    var path = "D:\\test2.vpc";
    if (File.Exists(path))
    {
        var vp = new VPContainer(path);
        //Any compressed file will be decompressed during extract
        await vp.ExtractVpAsync("D:\\extractTest");
    }
}

async Task CreateCompressedVPFromFolder()
{
    var path = "D:\\extract";
    if (Directory.Exists(path))
    {
        var vp = new VPContainer();
        vp.AddFolderToRoot(path);
        vp.EnableCompression();
        await vp.SaveAsAsync("D:\\test2.vpc");
    }
}

async Task CreateVPFromFolder()
{
    var path = "D:\\extract";
    if (Directory.Exists(path))
    {
        var vp = new VPContainer();
        vp.AddFolderToRoot(path);
        await vp.SaveAsAsync("D:\\test.vp");
    }
}

void DisplayVPData()
{
    var path = "D:\\test2.vpc";
    if (File.Exists(path))
    {
        var VP = new VPContainer(path);
        PrintFileTreeRecursive(VP.vpFiles);
    }
}


async Task DisplayVPDataAsync()
{
    var path = "D:\\mv_assets.vpc";
    if (File.Exists(path))
    {
        var VP = new VPContainer();
        await VP.LoadVP(path);
        PrintFileTreeRecursive(VP.vpFiles);
    }
}

void PrintFileTreeRecursive(List<VPFile> fileList)
{
    foreach (var file in fileList)
    {
        if (file.parent == null)
            Console.WriteLine("(" + file.type + ")" + file.info.name + " Parent: Null" + " Size: " + file.info.size + " Compressed?: "+file.compressionInfo.header + " T:" + VPTime.GetDateFromUnixTimeStamp(file.info.timestamp) + " Offset: " + file.info.offset);
        else
            Console.WriteLine("(" + file.type + ")" + file.info.name + " Parent: " + file.parent.info.name + " Size: " + file.info.size + " Compressed?: " + file.compressionInfo.header + " T:" + VPTime.GetDateFromUnixTimeStamp(file.info.timestamp) + " Offset: " + file.info.offset);
        if (file.files != null)
            PrintFileTreeRecursive(file.files);
    }
}
#pragma warning restore CS8321