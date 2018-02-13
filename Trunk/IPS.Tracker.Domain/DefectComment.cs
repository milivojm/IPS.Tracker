using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPS.Tracker.Domain
{
    public class DefectComment
    {
        public int Id { get; private set; }
        public int CommentatorId { get; set; }

        public DateTime CommentDate { get; set; }

        public int DefectId { get; set; }

        public string Text { get; set; }

        public virtual Defect Defect { get; private set; }

        public virtual Worker Commentator { get; private set; }

        public void AddChange(string propertyName, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(Text))
                Text = string.Format("{0}: {1} => {2}", propertyName, oldValue, newValue);
            else
                Text += string.Format(", {0}: {1} => {2}", propertyName, oldValue, newValue);
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Text);
        }

        public IEnumerable<int> GetLinkedDefectNumbers()
        {
            List<int> result = new List<int>();

            if (Text != null)
            {
                Regex r = new Regex(@"#(\d+)\b");
                MatchCollection matchCol = r.Matches(Text);

                foreach (Match m in matchCol)
                {
                    result.Add(int.Parse(m.Groups[1].Value));
                }
            }

            return result;
        }
    }
}
