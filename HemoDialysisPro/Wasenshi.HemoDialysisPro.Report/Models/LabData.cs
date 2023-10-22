using System.Collections.Generic;
using System.Diagnostics;
using Wasenshi.HemoDialysisPro.Models;

namespace Wasenshi.HemoDialysisPro.Report.Models
{
    public class LabData
    {
        protected Dictionary<string, LabExam> Labs = new Dictionary<string, LabExam>();

        public LabExam GetLab(string key)
        {
            if (Labs.TryGetValue(key, out LabExam labExam))
            {
                return labExam;
            }
            return null;
        }

        public void Add(string key, LabExam lab)
        {
            if (!Labs.TryAdd(key, lab))
            {
                Debug.WriteLine("couldn't add lab value to hemosheet");
            }
        }

        public void AddRange(IEnumerable<KeyValuePair<string, LabExam>> values)
        {
            foreach (KeyValuePair<string, LabExam> item in values)
            {
                Add(item.Key, item.Value);
            }
        }
    }
}
