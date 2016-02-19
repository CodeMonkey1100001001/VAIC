# VAIC
Verificare Archive Integrity Checker

VAIC is a program to compare to hashdeep files in an offline mode.

Usage: vaic.exe - Archive Hash Integrity Checker
version: 0.1
Main Modes:
  -s    Set source hash file (-s=/tmp/source_dir.hash)
  -d    Set destination hash file (-d=/tmp/destination_dir.hash)
  -a    Action to perform (-a=AUDIT)
        AUDIT - only create an index of Source Directory (equiv to find ./dir/ >files.txt)
  -i    Ignore Matches. Only show anomalous files.
  -m    use MD5 Sum for match.
  -2    use SHA256 Sum for match.
  -h    This help
  -v    Set Verbosity Level (-v=1)
Usage Examples:
 
vaic.exe -h
vaic.exe -s=/tmp/source_dir.hash -d=/tmp/destination_dir.hash -a AUDIT 
