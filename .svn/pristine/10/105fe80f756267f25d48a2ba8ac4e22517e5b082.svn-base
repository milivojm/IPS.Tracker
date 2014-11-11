using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IPS.Tracker.SyncService.TrackerService;
using Microsoft.Exchange.WebServices.Data;

namespace IPS.Tracker.SyncService
{
    public partial class SyncService : ServiceBase
    {
        private ExchangeService _exchangeService;
        private TrackerService.TrackerServiceClient _client;
        private string _syncState = String.Empty;
        private List<WorkerDTO> _activeWorkers;
        private const string _gzrMailFormat = @"^(\w+)@gzr.hr$";
        private const string _hashTag = @"#(\d+)\b";
        private string _ipsPassword;
        private string _domena;
        private System.Timers.Timer _timer;

        public SyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _client = new TrackerService.TrackerServiceClient();
            _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
            _exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP2);

            if (args.Count() >= 1)
                _ipsPassword = args[0];
            else
                _ipsPassword = "Rijeka051";

            if (args.Count() == 2)
                _domena = args[1];
            else
                _domena = "domena";

            _exchangeService.Credentials = new NetworkCredential("ips", _ipsPassword, _domena);
            _exchangeService.AutodiscoverUrl("ips@gzr.hr", RedirectionUrlValidationCallback);

            if (_exchangeService.Url == null)
            {
                base.EventLog.WriteEntry("Ne mogu dohvatiti Url od Exchange Web Services", EventLogEntryType.Error);
                base.Stop();
            }
            else
            {
                Thread t = new Thread(new ThreadStart(this.InitTimer));
                t.Start();
            }
        }

        private void InitTimer()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += _timer_Elapsed;
            _timer.Interval = 60000;
            _timer.Enabled = true;
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Sync();
        }

        protected override void OnStop()
        {
            _timer.Enabled = false;
        }

        private void Sync()
        {
            try
            {
                PropertySet pSet = new PropertySet(BasePropertySet.FirstClassProperties);
                pSet.RequestedBodyType = BodyType.Text;
                Regex regex = new Regex(_hashTag);
                _activeWorkers = _client.GetActiveWorkers();

                ChangeCollection<ItemChange> itemChangeCollection = _exchangeService.SyncFolderItems(new FolderId(WellKnownFolderName.Inbox), pSet, null, 20, SyncFolderItemsScope.NormalItems, _syncState);
                _syncState = itemChangeCollection.SyncState;

                foreach (ItemChange ic in itemChangeCollection)
                {
                    if (ic.Item is EmailMessage)
                    {
                        EmailMessage msg = (EmailMessage)ic.Item;
                        msg.Load(pSet);
                        Match match = regex.Match(msg.Subject);

                        if (match.Success)
                        {
                            string tag = match.Groups[1].Value;
                            DefectDTO dto = _client.GetDefectById(int.Parse(tag));

                            if (dto.Id != 0)
                            {
                                InsertNewComment(msg);
                            }
                            else
                            {
                                InsertNewDefect(msg);
                            }
                        }
                        else
                        {
                            InsertNewDefect(msg);
                        }

                        msg.Delete(DeleteMode.MoveToDeletedItems);
                    }
                }
            }
            catch (Exception e)
            {
                this.EventLog.WriteEntry(e.Message, EventLogEntryType.Error);
            }
        }

        private void InsertNewDefect(EmailMessage msg)
        {
            string reporterAddress = msg.From.Address;
            Match m = Regex.Match(reporterAddress, _gzrMailFormat);

            if (m.Success)
            {
                string reporterUsername = m.Groups[1].Value;
                WorkerDTO reporter = _activeWorkers.FirstOrDefault(w => w.Username.ToLower() == reporterUsername.ToLower());

                if (reporter != null)
                {
                    _client.ReportNewDefect(msg.Subject, msg.Body, null, reporter.Id, reporter.Id, 0, null);
                }
            }
        }

        private void InsertNewComment(EmailMessage msg)
        {
            string reporterAddress = msg.From.Address;
            Match m = Regex.Match(reporterAddress, _gzrMailFormat);

            if (m.Success)
            {
                string reporterUsername = m.Groups[1].Value;
                WorkerDTO reporter = _activeWorkers.FirstOrDefault(w => w.Username.ToLower() == reporterUsername.ToLower());

                if (reporter != null)
                {
                    Match hashMatch = Regex.Match(msg.Subject, _hashTag);

                    if (hashMatch.Success)
                    {
                        int taskId = int.Parse(hashMatch.Groups[1].Value);

                        Match commentMatch = Regex.Match(msg.Body, "^(?<comment>(.|\n)+)From: ips");

                        if (commentMatch.Success)
                        {
                            string comment = commentMatch.Groups["comment"].Value;
                            _client.CommentOnDefect(taskId, reporter.Id, comment);
                        }
                    }
                }
            }
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }

            return result;
        }
    }
}
