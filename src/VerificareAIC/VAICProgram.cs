using System;


//Verificare - To verify
//AIC Archive Integrity Checker
//Or VAIC
namespace VerificareAIC
{
    class MainClass
    {
        enum ExitCode : int 
        {
            PASSED = 0,
            FAILED = 1,
            UNKNOWN = 2
        }




        public static int Main(string[] args)
        {
            double version=0.1;
            int verbosity = 1;

            //debug("Hello World!");
            string sourceHash = "./TestData/";
            string destinationHash = "";

            string actionToPerform = "AUDIT";
            bool ignoreMatches = false;
            bool useMD5Sum = false;
            bool useSHA256Sum = false;

            foreach (string argValue in args)
            {
                //debug("Args[" + argValue + "]");
                switch (argValue.Substring(0, 2))
                {
                    case "-s":
                        sourceHash = argValue.Split('=')[1];
                        break;
                    case "-d":
                        destinationHash = argValue.Split('=')[1];
                        break;
                    case "-v":
                        verbosity = int.Parse(argValue.Split('=')[1]);
                        debug("Setting verbosity to [" + verbosity.ToString() + "]");
                        break;
                    case "-a":
                        actionToPerform = argValue.Split('=')[1];
                        break;
                    case "-i":
                        ignoreMatches = true;
                        break;
                    case "-m":
                        useMD5Sum = true;
                        break;
                    case "-2":
                        useSHA256Sum = true;
                        break;
                    case "-h":
                        debug("Usage: vaic.exe - Archive Hash Integrity Checker");
                        debug("version: " + version);
                        debug("Main Modes:");
                        debug("  -s    Set source hash file (-s=/tmp/source_dir.hash)");
                        debug("  -d    Set destination hash file (-d=/tmp/destination_dir.hash)");
                        debug("  -a    Action to perform (-a=AUDIT)");
                        debug("        AUDIT - only create an index of Source Directory (equiv to find ./dir/ >files.txt)");
                        debug("  -i    Ignore Matches. Only show anomalous files.");
                        debug("  -m    use MD5 Sum for match.");
                        debug("  -2    use SHA256 Sum for match.");

                        //                        debug("        INDEX - only create an index of Source Directory (equiv to find ./dir/ >files.txt)");
//                        debug("        SOURCETODEST - Copy files in Source Director to Destination Directory");
//                        debug("        SOURCEVERIFY - Veryify only files in source to known HASH.");
//                        debug("        DESTVERIFY   - Veryify destination to known HASH.");
//                        debug("  -S    Strict. Source and destinations are checked against HASH store.");
//                        debug("  -D    DataDirectory. Where to write HASH values (-D=/tmp/vaic/)");

//                        debug(" -[R|O] Resume operations or Overwrite Current run -R default");

                        debug("  -h    This help");
                        debug("  -v    Set Verbosity Level (-v=1)");
                        debug("Usage Examples:");
                        debug(" ");
                        debug("vaic.exe -h");
                        debug("vaic.exe -s=/tmp/source_dir.hash -d=/tmp/destination_dir.hash -a AUDIT ");
                        debug(" ");
                        System.Environment.Exit(0);
                    break;

                }//switch
            }//foreach
            debug("Action to perform: [" + actionToPerform + "]");
            debug("Source Hash      : [" + sourceHash + "]");
            debug("Destination Hash : [" + destinationHash + "]");
            bool auditPassed = false;

            VAIC currentVAIC = new VAIC();
            if (actionToPerform == "AUDIT")
            {
                currentVAIC.LoadSourceHash(sourceHash);
                currentVAIC.AuditDestination(destinationHash,useMD5Sum,useSHA256Sum);
                auditPassed=currentVAIC.ShowAuditReport(ignoreMatches);
            }

            if (auditPassed == false)
                return (int)ExitCode.FAILED;

            return (int)ExitCode.PASSED;


        }//Main
        public static void debug(string outval)
        {
            Console.WriteLine(outval);
        }
    }//class MainClass
}
