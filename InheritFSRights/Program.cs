using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InheritFSRights
{
    class Program
    {
        static void Main(string[] args)
        {
            try { 
            if (args.Length > 0)
            {
                foreach (String path in args)
                {
                    FSRights fs = new FSRights();
                    fs.path = path;
                    fs.recursive = true;
                    fs.parallelTask = true;
                    fs.run();
                }
            }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(1);
            }
            //Console.Read();
        }
    }
}
