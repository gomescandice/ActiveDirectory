using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSample.Responses
{
  public class BaseResponse<T>
  {
    public T response;
    public ADResponseType responseType;
    public string message;
    public string exceptionMessage;
    public string exceptionStackTrace;

    public BaseResponse(T responseResult, ADResponseType rt, string msg)
    {
      response = responseResult;
      responseType = rt;
      message = msg;
    }

    public BaseResponse(T responseResult, ADResponseType rt, string msg, Exception e) : this(responseResult, rt, msg)
    {
      if (null != e)
      {
        exceptionMessage = e.Message;
        exceptionStackTrace = e.StackTrace;
      }
    }

  }
}
