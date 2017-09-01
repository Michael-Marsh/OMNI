using OMNI.HDT.Model;
using OMNI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.HDT.ViewModel
{
    public class TicketFormViewModel : ViewModelBase
    {
        #region Properties

        public Ticket ITTicket { get; set; }

        #endregion

        public TicketFormViewModel()
        {

        }

        public TicketFormViewModel(Ticket ticket)
        {

        }

        /// <summary>
        /// Object disposal
        /// </summary>
        /// <param name="disposing">Called by the GC Finalizer</param>
        public override void OnDispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
