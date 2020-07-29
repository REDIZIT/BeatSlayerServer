using System;
using System.Collections.Generic;

namespace BeatSlayerServer.Enums
{
    public class OperationResult
    {
        public State state;
        public enum State { Success, Fail }

        public string message;
        public Exception exception;
        public List<string> details = new List<string>();

        public object obj;

        public OperationResult(Exception exception, string message = "")
        {
            this.exception = exception;
            this.message = message;
            state = State.Fail;
        }
        public OperationResult(State state, object obj)
        {
            this.state = state;
            this.obj = obj;
        }

        public OperationResult(State state, string message, List<string> details)
        {
            this.state = state;
            this.message = message;
            this.details = details;
        }
        public OperationResult(State state, string message)
        {
            this.state = state;
            this.message = message;
        }
        public OperationResult(State state)
        {
            this.state = state;
        }
    }

    public struct OperationResultCut
    {
        public OperationState state;
        public string message;

        public OperationResultCut(OperationState state, string message)
        {
            this.state = state;
            this.message = message;
        }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    public enum OperationState
    {
        Fail, Success
    }
}
