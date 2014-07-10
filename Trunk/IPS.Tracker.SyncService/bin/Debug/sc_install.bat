sc delete SyncService
sc create SyncService start= demand binPath= "c:\Projects\IPS.Tracker\Trunk\IPS.Tracker.SyncService\bin\Debug\IPS.Tracker.SyncService.exe" DisplayName= "IPS SyncService" obj= "domena\ips" password= "Rijeka051"
sc description SyncService "Monitors IPS mailbox for tracker messages and forwards them to IPS."
