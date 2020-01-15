using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleConnectV2.Command
{
    public enum CommsProtocol
    {
        NONE,
        RTU,
        ETHERNET
    }

    class CommandIdentify : ICommand
    {
        public string name { get; }

        public string[] reqArgs { get; }

        public string[] optArgs { get; }

        private string lastError = "";

        private CommsProtocol protocol;
        private string interfaceName;


        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CommandIdentify(string name)
        {
            this.name = name;
            this.reqArgs = new string[] { "PROTOCOL" };
            this.optArgs = new string[] { "INTERFACE" };
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

            if (protocol == CommsProtocol.NONE || protocol == CommsProtocol.RTU)
            {
                logger.Error("Comms Protocol RTU is not supported yet");
            }
            scApp.DiscoverDrives();
        }

        public bool isArgsValid(Dictionary<string, string> cmdLineArgs)
        {
            //Check that all required arguments exist
            foreach (string arg in reqArgs)
            {
                logger.Debug("Validating arg " + arg);
                if( !cmdLineArgs.Keys.Contains(arg) )
                {
                    this.lastError = "Argument Error : Command " + this.name + " Argument not found: " + arg;
                    return false;
                }
            }
            logger.Debug("Required Arguments are all valid");
            //Now check if any arguments specified do not exist in the optional argument list
            foreach (string arg in cmdLineArgs.Keys)
            {
                logger.Debug("Validation opt Arg : " + arg);
                if (!optArgs.Contains(arg) && !reqArgs.Contains(arg))
                {
                    this.lastError = "Argument Error : Command " + this.name + " Optional Argument not valid: " + arg;
                    return false;
                }
            }

            //Arguments are validated
            return true;
        }

        public string getLastError()
        {
            return this.lastError;
        }

        private bool validateArgs(Dictionary<string, string> cmdLineArgs)
        {
            protocol = CommsProtocol.NONE;

            if (cmdLineArgs[reqArgs[0]].ToUpper().Equals("RTU")) {
                //this is not supported yey only ethernet is supported
                logger.Debug("Using RTU Protocol");
                protocol = CommsProtocol.RTU;
            }
            else if (cmdLineArgs[reqArgs[0]].ToUpper().Equals("IP")) {
                logger.Debug("Using ETHERNET Protocol");
                protocol = CommsProtocol.ETHERNET;
            }
            else
            {
                logger.Debug("Inavlid protocol specified : " + cmdLineArgs[reqArgs[0]].ToUpper());
                this.lastError = "Unknown protocol " + cmdLineArgs[reqArgs[0]].ToUpper();
                return false;
            }

            if (protocol == CommsProtocol.NONE)
                return false;

            //get interface if specified
            if (cmdLineArgs.ContainsKey(optArgs[0]))
            {
                interfaceName = cmdLineArgs[optArgs[0]];
                logger.Debug("Interface is specified : " + interfaceName);
            }
            else
            {
                interfaceName = null;
            }

            return true;
        }
    }
}
