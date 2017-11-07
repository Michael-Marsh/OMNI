namespace OMNI.Testing
{
    public class DataPacket
    {
        #region Properties

        public object Obj { get; set; }
        public string ErrorMessage { get; set; }
        public string Message { get; set; }

        #endregion

        /// <summary>
        /// DataPacket Constructor
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="errorMsg">Error Message</param>
        /// <param name="msg">General Message</param>
        public DataPacket(object obj, string errorMsg = "", string msg = "")
        {
            Obj = obj;
            ErrorMessage = errorMsg;
            Message = msg;
        }
    }
}
