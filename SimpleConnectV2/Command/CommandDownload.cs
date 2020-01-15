using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace SimpleConnectV2.Command
{


    class CommandDownload : ICommand
    {
        public string name { get; }

        public string[] reqArgs { get; }

        public string[] optArgs { get; }

        private string lastError = "";

        private IPAddress ipAddress;
        private string filePath;
        private DownloadType type;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CommandDownload(string name)
        {
            this.name = name;
            this.reqArgs = new string[] { "IP", "FILE", "TYPE" };
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
            scApp.downloadFile(ipAddress, filePath, type);
            logger.Info("Downloand Complete");
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

        public string getLastError()
        {
            return this.lastError;
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

            filePath = cmdLineArgs[reqArgs[1]]; //FILE
            if (filePath == null || !(File.Exists(filePath)))
            {
                logger.Error("Sepcified File does not exist");
                return false;
            }

            type = DownloadType.NONE;

            string downloadType = cmdLineArgs[reqArgs[2]]; //TYPE
            if (downloadType == null)
            {
                logger.Error("Invalid Download Type Specified");
                return false;
            }

            switch (downloadType.ToUpper()) {
                case "PARAM":
                    type = DownloadType.PARAMETERS;
                    logger.Debug("Downalod Parameer File");
                    if (!Path.GetExtension(filePath).ToUpper().Equals(".PARFILE"))
                    {
                        logger.Error("Download Type is Parameter File, but a parameter file was not passed.");
                        return false;
                    }
                    break;
                case "MACRO":
                    type = DownloadType.MACRO;
                    logger.Debug("Downalod Macro File");
                    if (!Path.GetExtension(filePath).ToUpper().Equals(".MACRO"))
                    {
                        logger.Error("Download Type is Macro File, but a Macro file was not passed.");
                        return false;
                    }
                    break;
                case  "ONBOARD":
                    type = DownloadType.ONBOARD;
                    logger.Debug("Downalod Onboard PLC File");
                    if (!Path.GetExtension(filePath).ToUpper().Equals(".MACRO"))
                    {
                        logger.Error("Download Type is Macro File, but a Macro file was not passed.");
                        return false;
                    }
                    break;
                case  "FIRMWARE":
                    type = DownloadType.FIRMWARE;
                    logger.Debug("Downalod Firnware");
                    
                    if (!Path.GetExtension(filePath).ToUpper().Equals(".IMG"))
                    {
                        logger.Error("Download Type is Firmware File, but a Firmware Image file was not passed.");
                        return false;
                    }

                    //Not uspported yet
                    logger.Error("Firmware download is not supported at this time");
                    return false;
                case  "MCI":
                    type = DownloadType.MCI;
                    logger.Debug("Downalod MCI PLC file");

                    if (!Path.GetExtension(filePath).ToUpper().Equals(".UBA"))
                    {
                        logger.Error("Download Type is MCI PLC Code File, but a Boot Application Image file was not passed.");
                        return false;
                    }

                    // Not support
                    logger.Error("MCI is not supported at this time");
                    return false;
            }
            return true;
        }
    }
}
