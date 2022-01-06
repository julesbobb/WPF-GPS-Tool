Imports Classes.Enums

Namespace Classes.Events

    Public Class ConnectionStateEventsArgs
        Inherits EventArgs

        Public State As SerialConnectionState

        Public Sub New(_State As SerialConnectionState)
            State = _State
        End Sub

    End Class

End Namespace