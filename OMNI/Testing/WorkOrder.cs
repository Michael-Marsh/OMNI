using System;

namespace OMNI.Testing
{
    public class WorkOrder
    {
        #region Properties

        public string ID { get; private set; }
        public string Seq { get; private set; }
        public string SeqDesc { get; private set; }
        public string PartNumber { get; private set; }
        public string WorkCenter { get; private set; }
        public string Desc { get; private set; }
        public int BalQty { get; private set; }
        public int ReqQty { get; private set; }

        #endregion

        /// <summary>
        /// Work Order Constructor
        /// </summary>
        public WorkOrder()
        {

        }

        /// <summary>
        /// Work Order Constructor
        /// </summary>
        /// <param name="id">Work Order ID</param>
        /// <param name="seq">Production sequence</param>
        /// <param name="seqDesc">Sequence description</param>
        /// <param name="partNumber">Part Number</param>
        /// <param name="workCenter">Work Center</param>
        /// <param name="desc">Work Order description</param>
        /// <param name="balQty">Quantity left to run or Balance quantity</param>
        /// <param name="reqQty">Intial required quantity to run</param>
        public WorkOrder(string id, string seq, string seqDesc, string partNumber, string workCenter, string desc, int balQty, int reqQty)
        {
            ID = id;
            Seq = seq;
            SeqDesc = seqDesc;
            PartNumber = partNumber;
            WorkCenter = workCenter;
            Desc = desc;
            BalQty = balQty;
            ReqQty = reqQty;
        }

        /// <summary>
        /// Work Order Constructor
        /// </summary>
        /// <param name="id">Work Order ID</param>
        /// <param name="seq">Production sequence</param>
        /// <param name="seqDesc">Sequence description</param>
        /// <param name="partNumber">Part Number</param>
        /// <param name="workCenter">Work Center</param>
        /// <param name="desc">Work Order description</param>
        /// <param name="balQty">Quantity left to run or Balance quantity</param>
        /// <param name="reqQty">Intial required quantity to run</param>
        public WorkOrder(string id, string seq, string seqDesc, string partNumber, string workCenter, string desc, string balQty, string reqQty)
        {
            ID = id;
            Seq = seq;
            SeqDesc = seqDesc;
            PartNumber = partNumber;
            WorkCenter = workCenter;
            Desc = desc;
            BalQty = Int32.TryParse(balQty, out int _balQty) ? _balQty : 0;
            ReqQty = Int32.TryParse(reqQty, out int _reqQty) ? _reqQty : 0;
        }
    }
}
