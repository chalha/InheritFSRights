using System;
using System.DirectoryServices.AccountManagement;

using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Diagnostics;
//using Alphaleonis.Win32.Filesystem;
using System.IO;

namespace InheritFSRights
{
    class FSRights
    {
        private int permissionCount = -1;
        //private SecurityIdentifier groupSid = null;
        public String path { get; set; }
        public Boolean recursive { get; set; } = false;
        public Boolean parallelTask { get; set; } = false;

        public FSRights()
        {
            init();
        }

        public void init()
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            /*GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, "Groupname");
            groupSid = group.Sid;*/
        }

        public void run()
        {
            doWork(path);
        }

        public void doWork(String path)
        {
            String[] subFiles=Directory.GetFiles(path);
            if (parallelTask)
            {
                Parallel.ForEach(subFiles,(file)=>{ inheritFilePermission(file); });
            }
            else
            {
                foreach(String file in subFiles)
                {
                    inheritFilePermission(file);
                }
            }
            String[] subDirectories = Directory.GetDirectories(path);
            if (parallelTask)
            {
                Parallel.ForEach(subDirectories, (folder) => { inheritDirectoryPermission(folder);
                    if (recursive)
                    {
                        DirectoryInfo info = new DirectoryInfo(folder);
                        if (!info.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint))
                        {
                            doWork(folder);
                        }
                    }
                });
            }
            else
            {
                foreach (String folder in subDirectories)
                {
                    inheritDirectoryPermission(folder);
                    if (recursive)
                    {
                        DirectoryInfo info = new DirectoryInfo(folder);
                        if (!info.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint)) { 
                            doWork(folder);
                        }
                    }
                }
            }
        }

        public void inheritFilePermission(String file)
        {
            Console.WriteLine("work with file " + file);

            FileSecurity fileSecurity;
            AuthorizationRuleCollection fileRules;
            //FileSystemAccessRule fileRule;
            FileInfo fileInfo = new FileInfo(file);
            try {
                fileSecurity = fileInfo.GetAccessControl();
            }
            catch(UnauthorizedAccessException e)
            {
                takeOwnershipOfFile(file);
                fileSecurity = File.GetAccessControl(file);
            }
            fileRules = fileSecurity.GetAccessRules(includeExplicit: true,
                         includeInherited: false, targetType: typeof(System.Security.Principal.SecurityIdentifier));
           // if (fileRules.Count != 0 || fileSecurity.GetAccessRules(false, true, typeof(System.Security.Principal.SecurityIdentifier)).Count == 0)
           // {
                fileSecurity.SetAccessRuleProtection(false, false);
                foreach (FileSystemAccessRule rule in fileRules)
                {
                    /*
                     * Remove any explicit permissions so we are just left with inherited ones.
                     */
                    fileSecurity.RemoveAccessRule(rule);
                }
                File.SetAccessControl(file,fileSecurity);
                Console.WriteLine("Enable Inheration " + file);
            //}
        }

        public void takeOwnershipOfFile(String file)
        {
            Console.WriteLine("TakeOwnership "+file);
            using (var user = WindowsIdentity.GetCurrent())
            {
                var ownerSecurity = new FileSecurity();
                ownerSecurity.SetOwner(user.User);
                File.SetAccessControl(file, ownerSecurity);

                var accessSecurity = new FileSecurity();
                accessSecurity.AddAccessRule(new FileSystemAccessRule(user.User, FileSystemRights.FullControl, AccessControlType.Allow));
                File.SetAccessControl(file, accessSecurity);
            }
        }

        public void inheritDirectoryPermission(String folder)
        {
            Console.WriteLine("work with folder " + folder);
            DirectorySecurity directorySecurity;
            AuthorizationRuleCollection directoryRules;
            //FileSystemAccessRule directoryRule;
            try
            {
                directorySecurity = Directory.GetAccessControl(folder);
            }
            catch (UnauthorizedAccessException e)
            {
                takeOwnershipOfDirectory(folder);
                directorySecurity = Directory.GetAccessControl(folder);
            }
            
            directoryRules = directorySecurity.GetAccessRules(includeExplicit: true,
                         includeInherited: false, targetType: typeof(System.Security.Principal.SecurityIdentifier));
            //if(directoryRules.Count!=0||directorySecurity.GetAccessRules(false,true, typeof(System.Security.Principal.SecurityIdentifier)).Count == 0)
            //{
                directorySecurity.SetAccessRuleProtection(false, false);
                foreach (FileSystemAccessRule rule in directoryRules)
                {
                /*
                 * Remove any explicit permissions so we are just left with inherited ones.
                 */
                    directorySecurity.RemoveAccessRule(rule);
                }
                Directory.SetAccessControl(folder,directorySecurity);
                Console.WriteLine("Enable Inheration " + folder);
            //}
        }


        public void takeOwnershipOfDirectory(String folder)
        {
            Console.WriteLine("TakeOwnership "+folder);
            using (var user = WindowsIdentity.GetCurrent())
            {
                var ownerSecurity = new DirectorySecurity();
                ownerSecurity.SetOwner(user.User);
                Directory.SetAccessControl(folder, ownerSecurity);

                var accessSecurity = new DirectorySecurity();
                accessSecurity.AddAccessRule(new FileSystemAccessRule(user.User, FileSystemRights.FullControl, AccessControlType.Allow));
                Directory.SetAccessControl(folder, accessSecurity);
            }
        }
    }

}
