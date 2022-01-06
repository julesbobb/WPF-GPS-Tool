
Namespace Classes.Events
    ''' <summary>
    ''' EventArgs used to send bytes received on serial port
    ''' </summary>
    Public Class SerialDataEventArgs
        Inherits EventArgs
        Public Sub New(dataInByteArray As Byte())
            Data = dataInByteArray
        End Sub

        ''' <summary>
        ''' Byte array containing data from serial port
        ''' </summary>
        Public Data As Byte()

        Public Sub New(line As String)
            ReadLine = line
        End Sub

        Public ReadLine As String

    End Class
End Namespace
