using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleConnectV2.Command
{
    class CommandUpload : ICommand
    {
        public string name { get; }

        public string[] reqArgs { get; }

        public string[] optArgs { get; }

        private string lastError = "";


        private IPAddress ipAddress;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CommandUpload(string name)
        {
            this.name = name;
            this.reqArgs = new string[] { "IP" };
            this.optArgs = new string[] { null };
        }
        public void DoAction(Dictionary<string, string> cmdLineArgs, SimpleConnect scApp)
        {
            if (!validateArgs(cmdLineArgs))
            {
                Console.WriteLine("ERROR" + getLastError());
                return;
            }

            //protocol and interfaceName will now be populated
            if (scApp == null)
            {
                logger.Error("SimpleConnect was not set");
                return;
            }
            //scApp.saveParameterFile("192.168.1.100");
            scApp.saveParameterFile(ipAddress);
            logger.Info("Upload Complete");
        }

        public bool isArgsValid(Dictionary<string, string> cmdLineArgs)
        {
            //Check that all required arguments exist
            foreach (string arg in reqArgs)
            {
                if (!cmdLineArgs.Keys.Contains(arg))
                {
                    this.lastError = "Argument Error : Command " + this.name + " Argument not found: " + arg;
                    return false;
                }
            }

            //Now check if any arguments specified do not exist in the optional argument list
            foreach (string arg in cmdLineArgs.Keys)
            {
                if (!optArgs.Contains(arg) && !reqArgs.Contains(arg))
                {
                    this.lastError = "Argument Error : Command " + this.name + " Optional Argument not valid: " + arg;
                    return false;
                }
            }

            //Arguments are validated
            return true;
        }


        private bool validateArgs(Dictionary<string, string> cmdLineArgs)
        {
            //Get IP Address
            string ip = cmdLineArgs[reqArgs[0]]; //IP
            if (ip == null || !(IPAddress.TryParse(ip, out ipAddress)))
            {
                logger.Error("Parameter IP is not a valid IP Address");
                return false;
            }

            return true;
        }
        public string getLastError()
        {
            return this.lastError;
        }
    }
}
