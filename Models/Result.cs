using System;

namespace BeatSlayerServer.Models
{
    public abstract class Result<T>
    {
        public abstract ResultStatus Status { get; }



        public static Result<T> Success() { return new SuccessResult<T>(); }
        public static Result<T> Success(T payload) { return new SuccessResult<T>(payload); }


        public static Result<T> Fail(string message) { return new FailResult<T>(message); }
        public static Result<T> Fail(string message, Exception exception) { return new FailResult<T>(message, exception); }
    }





    public class SuccessResult<T> : Result<T>
    {
        public override ResultStatus Status => ResultStatus.Success;

        public T Payload { get; private set; }


        public SuccessResult() { }
        public SuccessResult(T payload)
        {
            Payload = payload;
        }
    }





    public class FailResult<T> : Result<T>
    {
        public override ResultStatus Status => ResultStatus.Fail;
        public string ErrorMessage { get; private set; }
        public Exception Error { get; private set; }



        public FailResult(string message)
        {
            ErrorMessage = message;
        }

        public FailResult(string messsage, Exception exception)
        {
            ErrorMessage = messsage;
            Error = exception;
        }
    }

    public enum ResultStatus
    {
        Fail, Success
    }
}
