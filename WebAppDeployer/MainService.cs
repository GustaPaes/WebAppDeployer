using Microsoft.Web.Administration;
using WebAppDeployer.Data.Enum;

namespace WebAppDeployer
{
    public static class MainService
    {
        private static bool _interactiveMode;
        public static void Process(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                ShowHelp();
                _interactiveMode = true;
            }
            else
            {
                if (args[0].Contains("WebAppDeployer.exe"))
                    for (int i = 0; i < args.Length - 1; i++)
                        args[i] = args[i + 1];
                try
                {
                    switch (args.Length)
                    {
                        case 1:
                            {
                                CheckAndCreateAppPool(args[0]);
                                break;
                            }
                        case 5:
                            {
                                CreateAppPoolAndApplication(args[0], args[1], args[2], args[3], args[4]);
                                break;
                            }
                    }
                    if (!_interactiveMode)
                        return;
                }
                catch (Exception ex)
                {
                    if (!_interactiveMode)
                        throw;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Environment.NewLine + ex.Message);
                }
                finally
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            string[] inputs = Console.ReadLine().Split(' ');
            inputs = inputs.Where(x => x != string.Empty).ToArray();
            Process(inputs);
        }

        private static void ShowHelp()
        {
            if (_interactiveMode) return;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("---------------- WebAppDeployer ---------------");
            Console.WriteLine("-----------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Uso: WebAppDeployer.exe [options]");

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("-----------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"WebAppDeployer.exe ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"<appPoolName>");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"<appPoolName> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"Nome da Pool para criação");
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("-----------------------------------------------");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"WebAppDeployer.exe ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"<appPoolName> <siteName> <virtualPath> <physicalPath> <concatString>?");
            Console.Write(Environment.NewLine);
            Console.Write($"<appPoolName> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"Nome da Pool para criação");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<siteName> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"Nome do Site do IIS para publicar");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<virtualPath> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"Caminho Virtual");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<physicalPath> = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"Caminho Fisico");
            Console.Write(Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"<concatString>? = ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"String para concatenação de POOL, não é obrigatório");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Environment.NewLine);
        }

        private static void CreateAppPoolAndApplication(string appPoolName, string siteName, string virtualPath, string physicalPath, string? concatString)
        {
            Console.WriteLine($"\nCriando a POOL e Aplicação IIS.");

            if (concatString != null && concatString != "")
                appPoolName = $"{concatString} - {appPoolName}";

            CheckAndCreateAppPool(appPoolName);

            bool createVirtualDirectory = virtualPath.Count(c => c == '/') >= 2;
            if (createVirtualDirectory)
            {
                string virtualDirectoryPath = virtualPath.Substring(0, virtualPath.LastIndexOf("/"));
                string parentPhysicalPath = Directory.GetParent(physicalPath).FullName;

                CheckAndCreateVirtualDirectory(siteName, virtualDirectoryPath, parentPhysicalPath);
            }

            Console.WriteLine($"POOL: {appPoolName}");
            Console.WriteLine($"Site: {siteName}");
            Console.WriteLine($"Aplicação: {virtualPath}");
            Console.WriteLine($"Pasta fisica: {physicalPath}");
            CheckAndCreateApplication(appPoolName, siteName, virtualPath, physicalPath);
        }

        private static void CheckAndCreateAppPool(string appPoolName)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ApplicationPool appPool = serverManager.ApplicationPools[appPoolName];
                if (appPool == null)
                {
                    appPool = serverManager.ApplicationPools.Add(appPoolName);
                    appPool.ManagedRuntimeVersion = ManagedRuntimeVersionEnum.NO_MANAGED_CODE.GetEnumDescription();
                    appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                    appPool.StartMode = StartMode.AlwaysRunning;
                    serverManager.CommitChanges();
                }
            }
        }

        private static void CheckAndCreateVirtualDirectory(string siteName, string virtualDirPath, string physicalPath)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Site site = serverManager.Sites[siteName];
                if (site != null)
                {
                    VirtualDirectory vDir = site.Applications["/"].VirtualDirectories[virtualDirPath];
                    if (vDir == null)
                    {
                        vDir = site.Applications["/"].VirtualDirectories.Add(virtualDirPath, physicalPath);
                        Console.WriteLine($"Criado novo Diretorio virtual: {virtualDirPath}.");
                        serverManager.CommitChanges();
                    }
                }
            }
        }

        private static void CheckAndCreateApplication(string appPoolName, string siteName, string virtualPath, string physicalPath)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Site site = serverManager.Sites[siteName];
                if (site != null)
                {
                    Application app = site.Applications[virtualPath];
                    if (app == null)
                    {
                        app = site.Applications.Add(virtualPath, physicalPath);
                        app.ApplicationPoolName = appPoolName;
                        app["preloadEnabled"] = true;
                        serverManager.CommitChanges();
                    }
                }
            }
        }
    }
}
