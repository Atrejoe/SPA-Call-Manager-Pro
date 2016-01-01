Imports PostSharp.Aspects

<assembly: ExceptionLogged()>

<Serializable>
Public Class ExceptionLoggedAttribute
    Inherits OnExceptionAspect

    Overrides Sub OnException(args As MethodExecutionArgs)
        args.Exception.Log()
        args.FlowBehavior = FlowBehavior.ThrowException
    End Sub


End Class
