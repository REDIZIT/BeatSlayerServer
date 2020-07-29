using BeatSlayerServer.Utils.Database;

namespace BeatSlayerServer.Utils
{
    public class OperationMessage
    {
        public OperationType Type { get; set; }
        public string Message { get; set; }
        public AccountData Account { get; set; }

        public OperationMessage() { }
        public OperationMessage(OperationType type)
        {
            Type = type;
        }
        public OperationMessage(OperationType type, string message)
        {
            Type = type;
            Message = message;
        }
        public OperationMessage(OperationType type, AccountData acc)
        {
            Type = type;
            Account = acc;
        }
    }
}
