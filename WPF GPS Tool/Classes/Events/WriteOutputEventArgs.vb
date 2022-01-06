Imports Classes.Enums

Namespace Classes.Events
    Public Class WriteOutputEventArgs
        Inherits EventArgs

        ''' <summary>
        ''' The output to use during the Environment.Exit command
        ''' </summary>
        Public Property Code As OutputCode

    End Class
End Namespace
