using OMNI.QMS.Model;
using OMNI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.QMS.ViewModel
{
    public class NoticeFilterViewModel : ViewModelBase
    {
        public List<string> FilterList { get; set; }
        private string selectedFilter;
        public string SelectedFilter
        {
            get { return selectedFilter; }
            set
            {
                switch(value)
                {
                    case "Mark All Viewed":
                        QIR.MarkAllViewed();
                        value = "Default";
                        break;
                    default:
                        value = "Default";
                        break;
                }
                selectedFilter = value;
                OnPropertyChanged(nameof(SelectedFilter));
            }
        }

        /// <summary>
        /// Notice Filter ViewModel Constructor
        /// </summary>
        public NoticeFilterViewModel()
        {
            if (FilterList == null)
            {
                FilterList = GetFilterList();
            }
            SelectedFilter = null;
        }

        public List<string> GetFilterList()
        {
            var _tempList = new List<string>
            {
                "Default",
                "Mark All Viewed",
                "Detailed Search"
            };
            return _tempList;
        }
    }
}
