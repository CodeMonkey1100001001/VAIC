using System;
using System.Collections.Generic;
using System.IO;
//using System.Collections;

namespace VerificareAIC
{
    public enum HashStatus: byte { NOTMatched, matched, HashMisMatch };

    public class HashEntry
    {
        public ulong fileSize { get; set; }
        public string md5 { get; set; }
        public string sha256 { get; set; }
        public HashStatus hashStatus { get; set; }
        public HashEntry(ulong theSize, string theMD5, string theSHA256)
        {
            fileSize=theSize;
            md5=theMD5;
            sha256=theSHA256;
            hashStatus = HashStatus.NOTMatched;
        }
        //Other properties, methods, events...
    }


    public class VAIC
    {
        Dictionary<string, HashEntry> hashEntries = new Dictionary<string, HashEntry>();
        Dictionary<string, HashEntry> hashEntriesNotFound = new Dictionary<string, HashEntry>();


        public VAIC()
        {
        }
        public bool LoadSourceHash(string filename)
        {
            debug("Loading Source hashes...");

            String line = "";
            int counter=0;
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            while((line = file.ReadLine()) != null)
            {

                if (line.StartsWith("%") == false && line.StartsWith("#") == false)
                {
                    string[] temps = line.Split(new char[] { ','}, 4);
                    HashEntry oneLineEntry = new HashEntry(ulong.Parse(temps[0]), temps[1], temps[2]);
                    hashEntries.Add(temps[3], oneLineEntry);
                    //debug(line);
                }
                counter++;
            }

            file.Close();

            return true;
        }

        public bool AuditDestination(string destFilename, bool useMD5Sum,bool useSHA256Sum)
        {
            debug("Audit Beginning...");
            if (useMD5Sum == false && useSHA256Sum == false) debug("############ WARNING No hashes being compared");
            if (useMD5Sum == true && useSHA256Sum == false)  debug("#######Warning: Using only MD5 Sum");

            String line = "";
            int counter=0;
            System.IO.StreamReader file = new System.IO.StreamReader(destFilename);
            while((line = file.ReadLine()) != null)
            {

                if (line.StartsWith("%") == false && line.StartsWith("#") == false)
                {
                    string[] temps = line.Split(new char[] { ','}, 4);
                    HashEntry oneLineEntry = new HashEntry(ulong.Parse(temps[0]), temps[1], temps[2]);
                    //hashEntries.Add(temps[3], oneLineEntry);
                    string lookingFor = temps[3];
                    //debug("Looking for:" + temps[3]);
                    try
                    {
                        if (hashEntries[lookingFor]!= null)
                        {
                            //debug(lookingFor+" FOUND");
                            hashEntries[lookingFor].hashStatus=HashStatus.matched;
                            if (useMD5Sum==true &&  hashEntries[lookingFor].md5 != oneLineEntry.md5) 
                            {
                                hashEntries[lookingFor].hashStatus=HashStatus.HashMisMatch;
                            }
                            if (useSHA256Sum==true &&  hashEntries[lookingFor].sha256 != oneLineEntry.sha256) 
                            {
                                hashEntries[lookingFor].hashStatus=HashStatus.HashMisMatch;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        hashEntriesNotFound.Add(lookingFor,oneLineEntry);
                        //debug(lookingFor+" not found in source.");
                        if (e.ToString() == null)
                            debug("weird");
                        //debug(e.ToString());
                    }
                }
                counter++;
            }

            file.Close();

            return true;
        }

        public bool ShowAuditReport(bool ignoreMatches)
        {
            bool auditPass = true;

            string extraAuditText = "";

            debug("Audit Report");
            foreach (KeyValuePair<string, HashEntry> pair in hashEntries)
            {
                if ( (pair.Value.hashStatus == HashStatus.matched) && (ignoreMatches==true) )
                {
//                        debug("ignore me: "+pair.Key + ": " + pair.Value.hashStatus.ToString());
                }
                else
                {
                    auditPass = false;

                    if (pair.Value.hashStatus == HashStatus.NOTMatched) extraAuditText = "File in Source but not in destination.";
                    if (pair.Value.hashStatus == HashStatus.HashMisMatch) extraAuditText = "Source and Destination HASH do not match.";
                    debug(pair.Key + ": " + pair.Value.hashStatus.ToString()+" : "+extraAuditText);
                }
                //Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
                extraAuditText = "";
            }
            //debug("In destination but not in source");
            foreach (KeyValuePair<string, HashEntry> pair in hashEntriesNotFound)
            {
                auditPass = false;
                if (pair.Value.hashStatus == HashStatus.NOTMatched) extraAuditText = "File in Destination but not in Source.";

                debug(pair.Key +":" + pair.Value.hashStatus.ToString()+" : "+extraAuditText);
                //Console.WriteLine("{0}, {1}", pair.Key, pair.Value);
            }
            if (auditPass == true)
            {
                debug("AUDIT: PASS");
            }
            else
            {
                debug("AUDIT: FAIL");
            }
            return auditPass;
        }

        public bool VAICReadDir(string sourceDir)
        {
            int one=1;
            if (one==1) one = 2;
            ProcessDirectory(sourceDir);
            return true;
        }
        void ProcessDirectory(string sourceDir)
        {
            Stack<string> stack;
            string[] files;
            string[] directories;
            string dir;

            stack = new Stack<string>();
            stack.Push(sourceDir);

            while (stack.Count > 0) {

                // Pop a directory
                dir = stack.Pop();

                files = Directory.GetFiles(dir);
                foreach(string file in files)
                {
                    FileInfo fileInfo= new FileInfo(file);

                    //debug("file=" + file);
                    nocrdebug(fileInfo.Length.ToString());
                    nocrdebug(","+MD5HashFile(file).ToLower());
                    nocrdebug(","+SHA256HashFile(file).ToLower());
                    debug("," + file);
                }

                directories = Directory.GetDirectories(dir);
                foreach(string directory in directories)
                {
                    // Push each directory into stack
                    debug("directory=" + directory);

                    stack.Push(directory);
                }
            }
        }//ProcessDirectory



        public bool AppendTextToFile(string theFilePath, string outData)
        {
            bool retV = false;
            try
            {
                //FileInfo fi = new FileInfo(thePath);

                if (!DirectoryExists(Path.GetDirectoryName(theFilePath)))
                {
                    CreateDirectory(Path.GetDirectoryName(theFilePath));
                }

                FileStream fileHandle = new FileStream(theFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);


                fileHandle.Seek(0, SeekOrigin.End);
                ///////////////// BAD BAD BAD...dont do this
                fileHandle.Write(System.Text.Encoding.ASCII.GetBytes(outData), 0, outData.Length);
                debug("Wrote " + outData.ToString() + ":" + theFilePath);
                fileHandle.Close();
                retV=true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to append to file Error: ["+theFilePath+"]\n" + e.ToString());

            }
            return retV;


        }//append all bytes

        public bool WriteTextToFile(string theFilePath, string outData)
        {
            bool retV = false;
            try 
            {
                if (!DirectoryExists(Path.GetDirectoryName(theFilePath)))
                {
                    CreateDirectory(Path.GetDirectoryName(theFilePath));
                }
                File.WriteAllText(theFilePath,outData);
                retV=true;
            }
            catch
            {
                debug("Unable to write to[" + theFilePath + "]");
            }
            return retV;
        }//writeTextToFile
        public bool DirectoryExists(string theFilePath)
        {
            bool retV = false;
            try
            {
                if (Directory.Exists(theFilePath)==true) retV=true;
            }
            catch
            {
                debug("Unknown. Unable to determine if dir exists["+theFilePath+"]");
            }
            return retV;
        }//directoryExists
        public bool CreateDirectory(string theFilePath)
        {
            bool retV = false;
            try
            {
                Directory.CreateDirectory(theFilePath);
                retV=true;
            }
            catch
            {
                debug("Unable to create Directory["+theFilePath+"]");
            }
            return retV;
        }//CreateDirectory

//        byte[] ComputeMD5Hash(string filePath)
//        {
//            using (var md5 = System.Security.Cryptography.MD5.Create())
//            {
//                //return md5.ComputeHash(File.ReadAllBytes(filePath));
//                return System.Text.Encoding.Default.GetString(md5.ComputeHash(filePath));
//            }
//        }
//        public string checkMD5(string filename)
//        {
//            using (var md5 = System.Security.Cryptography.MD5.Create())
//            {
//                using (var stream = File.OpenRead(filename))
//                {
//                    return System.Text.Encoding.ASCII.GetString(md5.ComputeHash(stream));
//                    //BitConverter.ToString(md5.ComputeHash(stream)).Replace("-","").ToLower(); –
//                }
//            }
//        }
        public string MD5HashFile(string fn)
        {            
            byte[] hash = System.Security.Cryptography.MD5.Create().ComputeHash(File.ReadAllBytes(fn));
            return BitConverter.ToString(hash).Replace("-", "");            
        }

        public string SHA256HashFile(string fn)
        {            
            byte[] hash = System.Security.Cryptography.SHA256.Create().ComputeHash(File.ReadAllBytes(fn));
            return BitConverter.ToString(hash).Replace("-", "");            
        }

        public void debug(string outStr)
        {
            Console.WriteLine(outStr);
        }
        public void nocrdebug(string outstr)
        {
            Console.Write(outstr);
        }


    }
}

