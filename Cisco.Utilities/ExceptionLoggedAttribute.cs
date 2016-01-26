using System;
using PostSharp.Aspects;

namespace Cisco.Utilities
{
    /// <summary>
    /// Logs an exception and rethrows it
    /// </summary>
    [Serializable()]
    public class ExceptionLoggedAttribute : OnExceptionAspect
    {

        /// <summary>
        /// Method executed <b>after</b> the body of methods to which this aspect is applied,
        /// in case that the method resulted with an exception (i.e., in a <c>catch</c> block).
        /// </summary>
        /// <param name="args">Advice arguments.</param>
        public override void OnException(MethodExecutionArgs args)
        {
            args.Exception.Log();
            args.FlowBehavior = FlowBehavior.ThrowException;
        }
    }
}
